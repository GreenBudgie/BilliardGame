using Godot;

public abstract partial class Ball : RigidBody2D
{
    [Export] public AudioStream BallHitSound;
    [Export] public AudioStream TableHitSound;
    [Export] public BallInfo BallInfo { get; private set; }

    [Signal]
    public delegate void PocketScoredEventHandler(Pocket pocket);

    // Damping
    private const float FastDampVelocityThreshold = 150;
    private const float FastDamp = 0.4f;
    private const float ExtremeDampVelocityThreshold = 40;
    private const float ExtremeDamp = 2f;
    private const float FastDampVelocityThresholdSq = FastDampVelocityThreshold * FastDampVelocityThreshold;
    private const float ExtremeDampVelocityThresholdSq = ExtremeDampVelocityThreshold * ExtremeDampVelocityThreshold;
    
    // Collision sounds
    private const float CollisionVolumeDbMin = -15f;
    private const float CollisionVolumeDbMax = 5f;
    private const float CollisionPitchMin = 0.75f;
    private const float CollisionPitchMax = 1.15f;
    private const float MinSoundVelocityThreshold = 20;
    private const float MaxSoundVelocityThreshold = 400;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        SleepingStateChanged += HandleSleepStateChange;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (LinearVelocity.IsZeroApprox())
        {
            return;
        }

        var velocitySq = LinearVelocity.LengthSquared();
        if (ApplyDamp(velocitySq, ExtremeDampVelocityThresholdSq, ExtremeDamp))
        {
            return;
        }

        if (ApplyDamp(velocitySq, FastDampVelocityThresholdSq, FastDamp))
        {
            return;
        }

        LinearDamp = 0;
        AngularDamp = 0;
    }

    private bool ApplyDamp(float velocity, float threshold, float damp)
    {
        if (velocity >= threshold)
        {
            return false;
        }

        LinearDamp = damp;
        AngularDamp = damp;
        return true;
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
        if (Sleeping)
        {
            EventBus.Instance.EmitSignal(EventBus.SignalName.BallStopped, this);
        }
    }
}