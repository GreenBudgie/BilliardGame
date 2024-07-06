using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class Ball : BallRigidBody
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

    protected abstract void RotateSprites(Vector4 finalRotation);

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        SleepingStateChanged += HandleSleepStateChange;
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

    private void OnBodyEntered(CollisionObject2D node)
    {
        if (node.GetCollisionLayerValue(3))
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
    
}