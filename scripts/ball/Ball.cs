using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class Ball : CharacterBody2D
{
    [Export] public AudioStream BallHitSound;
    [Export] public AudioStream TableHitSound;
    [Export] public BallInfo BallInfo { get; private set; }

    [Signal]
    public delegate void PocketScoredEventHandler(Pocket pocket);

    // Collision sounds
    private const float CollisionVolumeDbMin = -15f;
    private const float CollisionVolumeDbMax = 5f;
    private const float CollisionPitchMin = 0.75f;
    private const float CollisionPitchMax = 1.15f;
    private const float MinSoundVelocityThreshold = 20;
    private const float MaxSoundVelocityThreshold = 400;
    
    private Quaternion _rotation = Quaternion.Identity;

    public bool IsSleeping { get; set; } = true;
    public Vector2 LinearVelocity { get; set; }

    private ShapeCast2D _shapeCast;

    protected abstract void RotateSprites(Vector4 finalRotation);

    public override void _Ready()
    {
        _shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleRotation(delta);
    }

    private void HandleRotation(double delta)
    {
        if (IsSleeping || LinearVelocity.IsZeroApprox())
        {
            return;
        }

        var rotationAngle = LinearVelocity.Length() * 0.1f * (float)delta;
        var rotationAxis = LinearVelocity.Orthogonal().Normalized();
        var rotation3DAxis = new Vector3(0, rotationAxis.X, rotationAxis.Y);

        var newRotation = new Quaternion(rotation3DAxis, rotationAngle);
        _rotation *= newRotation;
        var rotationAsVector = new Vector4(_rotation.X, _rotation.Y, _rotation.Z, _rotation.W);
        
        RotateSprites(rotationAsVector);
    }

    private void OnBodyEntered(Node node)
    {
        if (node is not PhysicsBody2D physicsBody)
        {
            return;
        }

        if (physicsBody.GetCollisionLayerValue(3))
        {
            HandleCollision(TableHitSound);
            return;
        }
        
        if (node is Ball)
        {
            HandleCollision(BallHitSound);
        }

        if (node is PocketBody pocketBody)
        {
            EmitSignal(SignalName.PocketScored, pocketBody.Pocket);
        }
    }

    private void HandleCollision(AudioStream soundToPlay)
    {
        var velocity = LinearVelocity.Length();
        if (velocity < MinSoundVelocityThreshold)
        {
            return;
        }
        
        var velocityWeight = Mathf.Clamp(velocity / MaxSoundVelocityThreshold, 0, 1);
        var sound = SoundManager.Instance.PlayPositionalSound(this, soundToPlay);
        
        sound.VolumeDb = Mathf.Lerp(CollisionVolumeDbMin, CollisionVolumeDbMax, velocityWeight);
        sound.PitchScale = Mathf.Lerp(CollisionPitchMin, CollisionPitchMax, velocityWeight);
        sound.RandomPitchOffset(0.05f);
    }

    private void HandleSleepStateChange()
    {
        if (IsSleeping)
        {
            EventBus.Instance.EmitSignal(EventBus.SignalName.BallStopped, this);
        }
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
                var colliderVelocity = Vector2.Zero;
                if (collider is Ball ball)
                {
                    colliderVelocity = ball.LinearVelocity;
                }
                return new CollisionData(collider, normal, colliderVelocity);
            }
        ).ToList();
    }

    public void HandleNewCollision(double delta, CollisionData collision)
    {
        if (collision.Collider is not Ball ball)
        {
            HandleBorderCollision(collision.Normal);
            return;
        }
        
        HandleBallCollision(ball, collision);
    }

    private void HandleBorderCollision(Vector2 normal)
    {
        LinearVelocity = LinearVelocity.Bounce(normal);
    }

    private void HandleBallCollision(Ball ball, CollisionData collision)
    {
        var ballVector = GlobalPosition - ball.GlobalPosition;
        var velocityVector = LinearVelocity - collision.ColliderVelocity;
        var resultVelocity = LinearVelocity - 
                             velocityVector.Dot(ballVector) / ballVector.LengthSquared() * ballVector;
        LinearVelocity = resultVelocity;
    }

}