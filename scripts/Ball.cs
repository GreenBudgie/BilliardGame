using Godot;

namespace Billiard.scripts;

public abstract partial class Ball : RigidBody2D
{

    [Export] public float FastDampVelocityThreshold = 150;
    [Export] public float FastDamp = 0.5f;
    [Export] public float ExtremeDampVelocityThreshold = 50;
    [Export] public float ExtremeDamp = 2f;

    [Signal]
    public delegate void PocketScoredEventHandler(Pocket pocket);

    private float _fastDampVelocityThresholdSq;
    private float _extremeDampVelocityThresholdSq;

    public override void _Ready()
    {
        _extremeDampVelocityThresholdSq = ExtremeDampVelocityThreshold * ExtremeDampVelocityThreshold;
        _fastDampVelocityThresholdSq = FastDampVelocityThreshold * FastDampVelocityThreshold;
        
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (LinearVelocity.IsZeroApprox())
        {
            return;
        }
        
        var velocitySq = LinearVelocity.LengthSquared();
        if (ApplyDamp(velocitySq, _extremeDampVelocityThresholdSq, ExtremeDamp))
        {
            return;
        }
        if (ApplyDamp(velocitySq, _fastDampVelocityThresholdSq, FastDamp))
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
        if (node is Pocket pocket)
        {
            EmitSignal(SignalName.PocketScored, pocket);
        }
    }

}