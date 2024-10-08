﻿using Godot;

public abstract partial class Ball : RigidBody2D
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
    
    public float Radius { get; private set; }

    protected abstract void RotateSprites(Vector4 finalRotation);

    public override void _Ready()
    {
        base._Ready();

        var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        var circleShape = (CircleShape2D)collisionShape.Shape;
        Radius = circleShape.Radius;
        
        BodyEntered += OnBodyEntered;
        SleepingStateChanged += HandleSleepStateChange;
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleRotation(delta);
    }

    private void HandleRotation(double delta)
    {
        if (Sleeping || LinearVelocity.IsZeroApprox())
        {
            return;
        }

        var rotationAngle = LinearVelocity.Length() * 0.1f * (float)delta;
        var rotationAxis = LinearVelocity.Orthogonal().Normalized();
        var rotation3DAxis = new Vector3(rotationAxis.X, 0, rotationAxis.Y);

        var newRotation = new Quaternion(rotation3DAxis, rotationAngle);
        _rotation = newRotation * _rotation;
        var rotationAsVector = new Vector4(_rotation.X, _rotation.Y, _rotation.Z, _rotation.W);
        
        RotateSprites(rotationAsVector);
    }

    private void OnBodyEntered(Node node)
    {
        var collisionObject = (CollisionObject2D)node;
        if (collisionObject.GetCollisionLayerValue(3))
        {
            HandleCollision(TableHitSound);
            return;
        }
        
        if (collisionObject is Ball)
        {
            HandleCollision(BallHitSound);
        }

        if (collisionObject is PocketBody pocketBody)
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
        if (Sleeping)
        {
            BallManager.Instance.HandleBallStopped(this);
        }
    }
    
}