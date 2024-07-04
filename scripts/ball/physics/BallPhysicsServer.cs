using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BallPhysicsServer : Node
{
    private const string BallsGroupName = "balls";

    private float _linearDamp;
    private float _sleepThreshold;
    private float _sleepThresholdSq;
    private readonly Dictionary<NewBall, List<CollisionObject2D>> _handledCollisions = new();

    public override void _Ready()
    {
        _linearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        _sleepThreshold = ProjectSettings.GetSetting("physics/2d/sleep_threshold_linear").As<float>();
        _sleepThresholdSq = _sleepThreshold * _sleepThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        var balls = GetBalls();
        var ctx = new BallPhysicsContext(
            _linearDamp,
            _sleepThresholdSq
        );

        // Process movement
        balls.ForEach(ball => ball.HandleMovement(delta, ctx));

        // Get currently colliding bodies
        var collisionsByBall = balls.ToDictionary(ball => ball, ball => ball.GetCollisions());
        
        foreach (var ball in balls)
        {
            // Retrieve collisions that were not present on previous update
            var newCollisions = HandleCollisionsAndGetNewColliders(ball, collisionsByBall[ball]);
            foreach (var newCollision in newCollisions)
            {
                ball.HandleNewCollision(delta, newCollision);
            }
        }
    }

    private List<CollisionData> HandleCollisionsAndGetNewColliders(NewBall ball, List<CollisionData> collisions)
    {
        var collisionDataByCollider = collisions.ToDictionary(collision => collision.Collider, c => c);
        var oldColliders = _handledCollisions[ball];
        var currentColliders = collisions.Select(collision => collision.Collider).ToList();

        // If we had no old colliders, all new collisions should be handled
        if (oldColliders == null)
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
        return newColliders.Select(collider => collisionDataByCollider[collider]).ToList();
    }

    private List<NewBall> GetBalls()
    {
        return GetTree().GetNodesInGroup(BallsGroupName).Cast<NewBall>().ToList();
    }
}