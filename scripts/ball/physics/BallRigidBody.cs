using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class BallRigidBody : CharacterBody2D
{
    /*
     * Emitted right after the ball changes it's sleep state
     */
    [Signal]
    public delegate void SleepingStateChangedEventHandler();

    [Signal]
    public delegate void BodyEnteredEventHandler(CollisionObject2D body);

    private bool _isSleeping = true;

    public bool IsSleeping
    {
        get => _isSleeping;
        set
        {
            _isSleeping = value;
            EmitSignal(SignalName.SleepingStateChanged);
        }
    }

    public Vector2 LinearVelocity { get; set; }

    public float Radius { get; private set; }

    private ShapeCast2D _shapeCast;

    public override void _Ready()
    {
        base._Ready();

        _shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");

        var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        var circleShape = (CircleShape2D)collisionShape.Shape;
        Radius = circleShape.Radius;
    }

    public abstract bool AwakesOtherBalls();

    /// <summary>
    /// Awakes the body and updates it's velocity
    /// </summary>
    /// <param name="velocity">New velocity to set</param>
    public void SetLinearVelocity(Vector2 velocity)
    {
        IsSleeping = false;
        LinearVelocity = velocity;
    }

    /// <summary>
    /// Performs an actual movement step if the body is not sleeping. May put it to sleep if the threshold is reached.
    /// </summary>
    public void HandleMovement(double delta, float linearDamp, float sleepThresholdSq)
    {
        if (IsSleeping)
        {
            return;
        }

        // Move the ball
        Position += LinearVelocity * (float)delta;
        LinearVelocity *= (float)(1 - delta * linearDamp);

        // Put to sleep if threshold is reached
        if (LinearVelocity.LengthSquared() > sleepThresholdSq)
        {
            return;
        }

        LinearVelocity = Vector2.Zero;
        IsSleeping = true;
    }
    
    /// <summary>
    /// Returns the closest collision that this body is in contact with, or null if there are no collisions.
    /// </summary>
    public CollisionData? GetClosestCollision()
    {
        if (IsSleeping)
        {
            return null;
        }

        _shapeCast.ForceShapecastUpdate();

        if (!_shapeCast.IsColliding())
        {
            return null;
        }

        var collisionCount = _shapeCast.GetCollisionCount();
        var closestCollisionDistanceSq = float.MaxValue;
        CollisionData? closestCollision = null;
        for (var i = 0; i < collisionCount; i++)
        {
            var collider = (CollisionObject2D)_shapeCast.GetCollider(i);
            var normal = _shapeCast.GetCollisionNormal(i);
            var contactPoint = _shapeCast.GetCollisionPoint(i);
            var colliderVelocity = Vector2.Zero;
            var colliderPosition = contactPoint;
            if (collider is BallRigidBody ball)
            {
                colliderVelocity = ball.LinearVelocity;
                colliderPosition = ball.GlobalPosition;
            }

            var distance = GlobalPosition.DistanceSquaredTo(colliderPosition);
            if (closestCollisionDistanceSq <= distance)
            {
                continue;
            }

            closestCollisionDistanceSq = distance;
            closestCollision = new CollisionData(
                collider,
                colliderPosition,
                colliderVelocity,
                GlobalPosition,
                normal,
                contactPoint,
                collider is BallRigidBody
            );
        }

        return closestCollision;
    }

    public virtual void HandleNewCollision(CollisionData collision)
    {
        if (collision.Collider is not Ball)
        {
            HandleBorderCollision(collision.Normal);
        }
        else
        {
            HandleBallCollision(collision);
        }

        EmitSignal(SignalName.BodyEntered, collision.Collider);
    }

    public void EscapeOverlaps(CollisionData collision)
    {
        var normalOffset = collision.Normal * Radius;
        var centerVector = collision.CollisionPoint - GlobalPosition;
        var offsetVector = normalOffset + centerVector;
        if (collision.Collider is not Ball)
        {
            GlobalPosition += offsetVector;
            return;
        }

        GlobalPosition += offsetVector * 0.5f;
    }

    private void HandleBorderCollision(Vector2 normal)
    {
        LinearVelocity = LinearVelocity.Bounce(normal);
    }

    private void HandleBallCollision(CollisionData collision)
    {
        var ballVector = collision.BallPosition - collision.ColliderPosition;
        var velocityVector = LinearVelocity - collision.ColliderVelocity;
        LinearVelocity -= velocityVector.Dot(ballVector) / ballVector.LengthSquared() * ballVector;
    }
}