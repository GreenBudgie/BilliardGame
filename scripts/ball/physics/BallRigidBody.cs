using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class BallRigidBody : CharacterBody2D
{
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

    public void ApplyImpulse(Vector2 impulse)
    {
        IsSleeping = false;
        LinearVelocity = impulse * 5; // TODO remove * 5
    }

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
        List<CollisionData> collisions = new();
        for (var i = 0; i < collisionCount; i++)
        {
            var collider = (CollisionObject2D)_shapeCast.GetCollider(i);
            var normal = _shapeCast.GetCollisionNormal(i);
            var contactPoint = _shapeCast.GetCollisionPoint(i);
            var colliderVelocity = Vector2.Zero;
            var colliderPosition = contactPoint;
            if (collider is Ball ball)
            {
                colliderVelocity = ball.LinearVelocity;
                colliderPosition = ball.GlobalPosition;
            }

            var collision = new CollisionData(
                collider,
                colliderPosition,
                colliderVelocity,
                GlobalPosition,
                normal,
                contactPoint
            );
            collisions.Add(collision);
        }

        return collisions.MinBy(collision => GlobalPosition.DistanceSquaredTo(collision.ColliderPosition));
    }

    public void HandleNewCollision(CollisionData collision)
    {
        EmitSignal(SignalName.BodyEntered, collision.Collider);
        if (collision.Collider is not Ball)
        {
            HandleBorderCollision(collision.Normal);
            return;
        }

        HandleBallCollision(collision);
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
        // TODO check if just GlobalPosition is better
        var ballVector = collision.BallPosition - collision.ColliderPosition;
        var velocityVector = LinearVelocity - collision.ColliderVelocity;
        LinearVelocity -= velocityVector.Dot(ballVector) / ballVector.LengthSquared() * ballVector;
    }
}