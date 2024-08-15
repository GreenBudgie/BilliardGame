using System.Collections.Generic;
using Godot;

public partial class CueBall : Ball
{

    private Vector2 _initialGlobalPosition;

    public BallState State { get; private set; }

    private Sprite2D _ballSprite;

    public override void _Ready()
    {
        base._Ready();

        _ballSprite = GetNode<Sprite2D>("BallSprite");

        _initialGlobalPosition = GlobalPosition;

        PocketScored += _HandlePocketCollision;
        SleepingStateChanged += _MakeIdleIfSleeping;
        
        EventBus.Instance.ShotPerformed += _PerformShot;
    }

    protected override void RotateSprites(Vector4 finalRotation)
    {
        var ballSpriteMaterial = (ShaderMaterial)_ballSprite.Material;
        ballSpriteMaterial.SetShaderParameter("rotation", finalRotation);
    }

    private void _PerformShot(ShotData shotData)
    {
        var velocity = ShotStrengthUtil.GetVelocity(GlobalPosition, shotData);
        ApplyImpulse(velocity);
        State = BallState.Rolling;
    }

    private void _MakeIdleIfSleeping()
    {
        if (State == BallState.Rolling && Sleeping)
        {
            State = BallState.Idle;
        }
    }

    private void _HandlePocketCollision(Pocket pocket)
    {
        LinearVelocity = Vector2.Zero;
        Rotation = 0;
        GlobalPosition = _initialGlobalPosition;
        EventBus.Instance.EmitSignal(EventBus.SignalName.BallScored, this, pocket);
    }

    public enum BallState
    {
        Idle,
        Rolling,
        ShotPrepare,
        ShotAnimation
    }
}