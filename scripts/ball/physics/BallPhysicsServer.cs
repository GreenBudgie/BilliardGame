using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BallPhysicsServer : Node
{
    private const string BallsGroupName = "balls";

    private float _linearDamp;
    private float _sleepThresholdSq;
    private readonly Dictionary<Ball, List<CollisionObject2D>> _handledCollisions = new();

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
        
        var collisionsByBall = new Dictionary<Ball, List<CollisionData>>();
        foreach (var ball in balls)
        {
            // Get currently colliding bodies
            var collisions = ball.GetCollisions();

            collisions = collisions
                .OrderBy(collision => collision.InitialBallPosition.DistanceSquaredTo(collision.InitialColliderPosition))
                .ToList();
            
            // Awake every ball with which a collision have happened, so it can be processed later
            foreach (var collision in collisions)
            {
                if (collision.Collider is Ball collidingBall)
                {
                    collidingBall.IsSleeping = false;
                }
            }
            
            collisionsByBall[ball] = collisions;
        }

        var orderedCollisions = new Dictionary<int, List<KeyValuePair<Ball, CollisionData>>>();
        foreach (var ball in balls)
        {
            // Retrieve collisions that were not present on previous update
            var newCollisions = UpdateHandledCollisionsAndGetNewColliders(ball, collisionsByBall[ball]);
            for (var i = 0; i < newCollisions.Count; i++)
            {
                var newCollision = newCollisions[i];

                if (!orderedCollisions.ContainsKey(i))
                {
                    orderedCollisions[i] = new List<KeyValuePair<Ball, CollisionData>>();
                }

                orderedCollisions[i].Add(new KeyValuePair<Ball, CollisionData>(ball, newCollision));
            }
        }
        
        foreach (var (_, collisions) in orderedCollisions)
        {
            var velocityModifications = new Dictionary<Ball, Vector2>();
            foreach (var (ball, collision) in collisions)
            {
                velocityModifications.Add(ball, ball.HandleNewCollisionAndGetNewVelocity(collision));
            }
            
            foreach (var (ball, newVelocity) in velocityModifications)
            {
                ball.LinearVelocity = newVelocity;
            }
        }

        // Process movement at the end so positions can update before the next physics step
        balls.ForEach(ball => ball.HandleMovement(delta, _linearDamp, _sleepThresholdSq));
    }

    private List<CollisionData> UpdateHandledCollisionsAndGetNewColliders(Ball ball, List<CollisionData> collisions)
    {
        var collisionDataByCollider = collisions.ToDictionary(collision => collision.Collider, c => c);
        var oldColliders = _handledCollisions.GetValueOrDefault(ball);
        var currentColliders = collisions.Select(collision => collision.Collider).ToList();

        // If we had no old colliders, all new collisions should be handled
        if (oldColliders == null || oldColliders.Count == 0)
        {
            _handledCollisions[ball] = currentColliders;
            return currentColliders.Select(collider => collisionDataByCollider[collider]).ToList();
        }

        var newColliders = new List<CollisionObject2D>();
        foreach (var currentCollider in currentColliders)
        {
            if (oldColliders.Contains(currentCollider))
            {
                // Bodies are still touching since previous update, skip
                continue;
            }

            // We have a new collider
            newColliders.Add(currentCollider);
            oldColliders.Add(currentCollider);
        }

        // Remove bodies that no longer collide
        oldColliders.RemoveAll(oldCollider => !newColliders.Contains(oldCollider));
        if (oldColliders.Count == 0)
        {
            _handledCollisions.Remove(ball);
        }
        
        return newColliders.Select(collider => collisionDataByCollider[collider]).ToList();
    }

    private List<Ball> GetBalls()
    {
        return GetTree().GetNodesInGroup(BallsGroupName).Cast<Ball>().ToList();
    }
}