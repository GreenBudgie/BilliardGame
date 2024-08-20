using System.Collections.Generic;
using Godot;

public partial class CueBall : Ball
{
    
    private const float MinVelocity = 25;
    private const float MaxVelocity = 1400;

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
    }

    protected override void RotateSprites(Vector4 finalRotation)
    {
        var ballSpriteMaterial = (ShaderMaterial)_ballSprite.Material;
        ballSpriteMaterial.SetShaderParameter("rotation", finalRotation);
    }

    private void _PerformShot(ShotData shotData)
    {
        var velocity = GetVelocity(GlobalPosition, shotData);
        ApplyImpulse(velocity);
        State = BallState.Rolling;
    }
    
    private float GetVelocityMagnitudeForStrength(float strength)
    {
        if (strength == 0)
        {
            return MaxVelocity;
        }
        
        return Mathf.Lerp(MinVelocity, MaxVelocity, strength);
    }

    private Vector2 GetVelocity(Vector2 origin, ShotData shotData)
    {
        var shotVector = (shotData.AimPosition - origin).Normalized();
        return shotVector * GetVelocityMagnitudeForStrength(shotData.Strength);
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