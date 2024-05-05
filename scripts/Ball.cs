using Godot;

namespace Billiard.scripts;

public abstract partial class Ball : RigidBody2D
{

    [Export] public float FastDampVelocityThreshold = 150;
    [Export] public float FastDamp = 0.7f;

    [Signal]
    public delegate void PocketScoredEventHandler(Pocket pocket);

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (LinearVelocity.Length() < FastDampVelocityThreshold)
        {
            LinearDamp = FastDamp;
            AngularDamp = FastDamp;
        }
        else
        {
            LinearDamp = 0;
            AngularDamp = 0;
        }
    }

    private void OnBodyEntered(Node node)
    {
        if (node is Pocket pocket)
        {
            EmitSignal(SignalName.PocketScored, pocket);
        }
    }

}