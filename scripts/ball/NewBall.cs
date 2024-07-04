using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NewBall : CharacterBody2D
{
    public bool IsSleeping { get; private set; }
    public Vector2 LinearVelocity { get; set; }

    private ShapeCast2D _shapeCast;

    public override void _Ready()
    {
        _shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
    }

    public void HandleMovement(double delta, BallPhysicsContext ctx)
    {
        if (IsSleeping)
        {
            return;
        }

        // Move the ball
        Position += LinearVelocity * (float)delta;
        LinearVelocity *= (float)(1 - delta * ctx.LinearDamp);

        // Put to sleep if threshold is reached
        if (LinearVelocity.LengthSquared() > ctx.SleepThresholdSq)
        {
            return;
        }

        LinearVelocity = Vector2.Zero;
        IsSleeping = true;
    }

    public List<CollisionData> GetCollisions()
    {
        if (IsSleeping)
        {
            return new List<CollisionData>();
        }

        _shapeCast.ForceShapecastUpdate();

        if (!_shapeCast.IsColliding())
        {
            return new List<CollisionData>();
        }

        var collisionCount = _shapeCast.GetCollisionCount();
        var collisionIndices = Enumerable.Range(0, collisionCount);

        return collisionIndices.Select(i =>
            {
                var collider = (CollisionObject2D)_shapeCast.GetCollider(i);
                var normal = _shapeCast.GetCollisionNormal(i);
                return new CollisionData(collider, normal);
            }
        ).ToList();
    }

    public void HandleNewCollision(double delta, CollisionData collision)
    {
        if (collision.Collider is not NewBall)
        {
            HandleBorderCollision(collision.Normal);
        }
    }

    private void HandleBorderCollision(Vector2 normal)
    {
        LinearVelocity = LinearVelocity.Bounce(normal);
    }
}