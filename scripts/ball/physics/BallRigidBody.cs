using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

public abstract partial class BallRigidBody : CharacterBody2D
{
    /*
     * Emitted right after the ball changes its sleep state
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


    public override void _Ready()
    {
        base._Ready();

        var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        var circleShape = (CircleShape2D)collisionShape.Shape;
        Radius = circleShape.Radius;
    }

    public abstract bool AwakesOtherBalls();

    /// <summary>
    /// Awakes the body and updates its velocity
    /// </summary>
    /// <param name="velocity">New velocity to set</param>
    public void SetLinearVelocity(Vector2 velocity)
    {
        IsSleeping = false;
        LinearVelocity = velocity;
    }

    private Vector2 _previousRealLinearVelocity;

    /// <summary>
    /// Performs an actual movement step if the body is not sleeping. May put it to sleep if the threshold is reached.
    /// </summary>
    public void HandleMovement(double delta, float linearDamp, float sleepThresholdSq, KinematicCollision2D collision)
    {
        if (IsSleeping)
        {
            return;
        }
        
        // Move the ball
        var realLinearVelocity = LinearVelocity * (float)delta;
        if (collision == null)
        {
            Position += realLinearVelocity;
        }
        else
        {
            var collider = (CollisionObject2D)collision.GetCollider();
            if (collider is not BallRigidBody)
            {
                if (collider.GetCollisionLayerValue(4))
                {
                    HandlePocketCollision(realLinearVelocity, collision);
                }
                else
                {
                    HandleBorderCollision(realLinearVelocity, collision);
                }
            }
            else
            {
                HandleBallCollision(realLinearVelocity, collision);
            }
        }
        
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
    public KinematicCollision2D GetClosestCollision(double delta)
    {
        if (IsSleeping)
        {
            return null;
        }
        
        var realLinearVelocity = LinearVelocity * (float)delta;
        return MoveAndCollide(realLinearVelocity, true);
    }

    private void HandlePocketCollision(Vector2 realLinearVelocity, KinematicCollision2D collision)
    {
        Position += realLinearVelocity;
        LinearVelocity = Vector2.Zero;
    }

    private void HandleBorderCollision(Vector2 realLinearVelocity, KinematicCollision2D collision)
    {
        Position += collision.GetTravel() + collision.GetRemainder().Bounce(collision.GetNormal());
        
        LinearVelocity = LinearVelocity.Bounce(collision.GetNormal());
    }

    private void HandleBallCollision(Vector2 realLinearVelocity, KinematicCollision2D collision)
    {
        var collider = (CollisionObject2D)collision.GetCollider();
        var ballVector = collision.GetPosition() - collider.GlobalPosition;
        var velocityVector = LinearVelocity - collision.GetColliderVelocity();
        var resultVelocityModification = velocityVector.Dot(ballVector) / ballVector.LengthSquared() * ballVector;
        LinearVelocity -= resultVelocityModification;
    }
}