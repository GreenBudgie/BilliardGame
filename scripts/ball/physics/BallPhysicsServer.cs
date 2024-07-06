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
    }

    public override void _PhysicsProcess(double delta)
    {
        PerformPhysicsStepForBalls(delta, _balls);
    }
    
    private readonly Dictionary<BallRigidBody, CollisionData> _collisionByBall = new();
    private readonly List<BallRigidBody> _orderedBalls = new();

    public void PerformPhysicsStepForBalls(double delta, List<BallRigidBody> balls)
    {
        // Sleeping bodies are processed last (IsSleeping = false comes first)
        _orderedBalls.Clear();
        foreach (var ball in balls)
        {
            if (ball.IsSleeping)
            {
                _orderedBalls.Add(ball);
            }
            else
            {
                _orderedBalls.Insert(0, ball);
            }
        }

        _collisionByBall.Clear();
        foreach (var ball in _orderedBalls)
        {
            var collision = ball.GetClosestCollision();

            if (!collision.HasValue)
            {
                _handledCollisions.Remove(ball);
                continue;
            }

            // Awake ball with which a collision have happened, so it can be processed later
            if (ball.AwakesOtherBalls() && collision.Value.Collider is BallRigidBody collidingBall)
            {
                collidingBall.IsSleeping = false;
            }

            _collisionByBall[ball] = collision.Value;
        }
        
        foreach (var (ball, collision) in _collisionByBall)
        {
            ball.EscapeOverlaps(collision);
            if (ShouldProcessCollision(ball, collision))
            {
                ball.HandleNewCollision(collision);
            }
        }
        
        // Process movement at the end so positions can update before the next physics step
        foreach (var ball in _orderedBalls)
        {
            ball.HandleMovement(delta, _linearDamp, _sleepThresholdSq);
        }
    }

    private bool ShouldProcessCollision(BallRigidBody ball, CollisionData collision)
    {
        var oldCollider = _handledCollisions.GetValueOrDefault(ball);
        var currentCollider = collision.Collider;

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