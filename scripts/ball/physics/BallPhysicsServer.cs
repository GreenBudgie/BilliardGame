using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BallPhysicsServer : Node
{
    private const string BallsGroupName = "balls";

    private float _linearDamp;
    private float _sleepThresholdSq;
    private readonly Dictionary<BallRigidBody, CollisionObject2D> _handledCollisions = new();
    private readonly List<BallRigidBody> _balls = new();

    public static BallPhysicsServer Instance { get; private set; }

    public double DefaultDelta;

    public override void _Ready()
    {
        Instance = this;
        DefaultDelta = 1d / Engine.PhysicsTicksPerSecond;

        _linearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var sleepThreshold = ProjectSettings.GetSetting("physics/2d/sleep_threshold_linear").As<float>();
        _sleepThresholdSq = sleepThreshold * sleepThreshold;
        
        _balls.AddRange(GetBalls());

        EventBus.Instance.BallScored += RemoveBallOnScore;
    }

    public override void _PhysicsProcess(double delta)
    {
        PerformPhysicsStepForBalls(delta, _balls);
    }

    private readonly Dictionary<BallRigidBody, KinematicCollision2D> _collisionByBall = new();
    private readonly List<BallRigidBody> _orderedBalls = new();
    private readonly Dictionary<BallRigidBody, KinematicCollision2D> _results = new();

    public Dictionary<BallRigidBody, KinematicCollision2D> PerformPhysicsStepForBalls(double delta, List<BallRigidBody> balls)
    {
        // Sleeping bodies are processed last (IsSleeping = false comes first)
        _orderedBalls.Clear();
        _results.Clear();
        var allBallsSleep = true;
        foreach (var ball in balls)
        {
            if (ball.IsSleeping)
            {
                _orderedBalls.Add(ball);
            }
            else
            {
                allBallsSleep = false;
                _orderedBalls.Insert(0, ball);
            }
        }

        if (allBallsSleep)
        {
            return _results;
        }

        _collisionByBall.Clear();
        foreach (var ball in _orderedBalls)
        {
            var collision = ball.GetClosestCollision(delta);

            if (collision == null)
            {
                _handledCollisions.Remove(ball);
                continue;
            }

            // Awake ball with which a collision have happened, so it can be processed later
            if (ball.AwakesOtherBalls() && collision.GetCollider() is BallRigidBody collidingBall)
            {
                collidingBall.IsSleeping = false;
            }

            _collisionByBall[ball] = collision;
            _results[ball] = collision;
        }
        
        // foreach (var (ball, collision) in _collisionByBall)
        // {
        //     //ball.EscapeOverlaps(collision);
        //     if (ShouldProcessCollision(ball, collision))
        //     {
        //         _results.Add(ball, ball.HandleNewCollision(collision));
        //     }
        // }
        
        // Process movement at the end so positions can update before the next physics step
        foreach (var ball in _orderedBalls)
        {
            ball.HandleMovement(delta, _linearDamp, _sleepThresholdSq, _collisionByBall.GetValueOrDefault(ball));
        }

        return _results;
    }

    private void RemoveBallOnScore(Ball ball, Pocket pocket)
    {
        if (ball is PocketBall)
        {
            _balls.Remove(ball);
        }
    }

    private bool ShouldProcessCollision(BallRigidBody ball, KinematicCollision2D collision)
    {
        var oldCollider = _handledCollisions.GetValueOrDefault(ball);
        var currentCollider = (CollisionObject2D)collision.GetCollider();

        if (oldCollider == currentCollider)
        {
            // Bodies are still touching since previous update, skip
            return false;
        }
        
        _handledCollisions[ball] = currentCollider;
        return true;
    }

    private List<BallRigidBody> GetBalls()
    {
        return GetTree().GetNodesInGroup(BallsGroupName).Cast<BallRigidBody>().ToList();
    }
}