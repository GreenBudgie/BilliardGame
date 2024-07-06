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

    public override void _Ready()
    {
        _linearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var sleepThreshold = ProjectSettings.GetSetting("physics/2d/sleep_threshold_linear").As<float>();
        _sleepThresholdSq = sleepThreshold * sleepThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        var balls = GetBalls();

        // Sleeping bodies are processed last, so the order is descending (IsSleeping = false comes first)
        balls = balls.OrderBy(ball => ball.IsSleeping).ToList();
        
        var collisionByBall = new Dictionary<BallRigidBody, CollisionData>();
        foreach (var ball in balls)
        {
            var collision = ball.GetClosestCollision();

            if (!collision.HasValue)
            {
                _handledCollisions.Remove(ball);
                continue;
            }

            // Awake ball with which a collision have happened, so it can be processed later
            if (collision.Value.Collider is BallRigidBody collidingBall)
            {
                collidingBall.IsSleeping = false;
            }

            collisionByBall[ball] = collision.Value;
        }
        
        foreach (var (ball, collision) in collisionByBall)
        {
            ball.EscapeOverlaps(collision);
            if (ShouldProcessCollision(ball, collision))
            {
                ball.HandleNewCollision(collision);
            }
        }

        // Process movement at the end so positions can update before the next physics step
        balls.ForEach(ball => ball.HandleMovement(delta, _linearDamp, _sleepThresholdSq));
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

    private List<Ball> GetBalls()
    {
        return GetTree().GetNodesInGroup(BallsGroupName).Cast<Ball>().ToList();
    }
}