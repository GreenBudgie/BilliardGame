using System.Collections.Generic;
using Godot;

public partial class CueBall : Ball
{
    private const string ShootAction = "shoot";
    private const string InverseShootAction = "inverse_shoot";

    [Signal]
    public delegate void ShotInitializedEventHandler();

    private Vector2 _initialGlobalPosition;

    public BallState State { get; private set; }

    public bool IsBallHovered { get; private set; }

    public ShotData ShotData { get; private set; } = new(Vector2.Zero, false, 0);

    private Sprite2D _ballSprite;

    public override void _Ready()
    {
        base._Ready();

        _ballSprite = GetNode<Sprite2D>("BallSprite");

        _initialGlobalPosition = Transform.Origin;

        PocketScored += HandlePocketCollision;
        SleepingStateChanged += MakeIdleIfSleeping;
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        IsBallHovered = mousePosition.DistanceSquaredTo(GlobalPosition) <= Radius * Radius;

        if (ShouldCancelShot())
        {
            State = BallState.Idle;
        }

        var allowedToShootByGameState =
            GameManager.Game.GameStateManager.State == GameState.ShotPreparation;
        var ableToShoot = State == BallState.ShotPrepare || IsBallHovered;

        if (!allowedToShootByGameState || !ableToShoot)
        {
            Input.SetDefaultCursorShape();
            return;
        }

        Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
        var shotData = ShotData;

        if (State == BallState.Idle && IsBallHovered && ShotPressed())
        {
            State = BallState.ShotPrepare;
            var isInverse = Input.IsActionJustPressed(InverseShootAction);
            ShotData = ShotData with { Inverse = isInverse };
        }

        if (State != BallState.ShotPrepare)
        {
            return;
        }

        var inverseShotSign = shotData.Inverse ? -1 : 1;
        var pullVector = (mousePosition - GlobalPosition) * inverseShotSign;

        var strength = ShotStrengthUtil.GetStrengthForPullVectorLength(pullVector.Length());

        ShotData = ShotData with
        {
            PullVector = pullVector,
            Strength = strength
        };

        if (strength > 0)
        {
            var travelDistance = ShotStrengthUtil.GetMaxTravelDistanceByStrength(this, strength);
        }

        if (ShouldPerformShot())
        {
            State = BallState.ShotAnimation;
            GameManager.Game.GameStateManager.ChangeState(GameState.ShotExecution);
            EmitSignal(SignalName.ShotInitialized);
        }
    }

    public void PerformShot()
    {
        var velocity = ShotStrengthUtil.GetImpulseForStrength(ShotData.Strength);
        SetLinearVelocity(ShotData.PullVector.Normalized() * velocity);
        State = BallState.Rolling;
    }

    protected override void RotateSprites(Vector4 finalRotation)
    {
        var ballSpriteMaterial = (ShaderMaterial)_ballSprite.Material;
        ballSpriteMaterial.SetShaderParameter("rotation", finalRotation);
    }
    
    private bool ShotPressed()
    {
        return Input.IsActionJustPressed(ShootAction) || Input.IsActionJustPressed(InverseShootAction);
    }

    private bool ShotReleased()
    {
        return Input.IsActionJustReleased(ShootAction) || Input.IsActionJustReleased(InverseShootAction);
    }

    private bool ShouldPerformShot()
    {
        return ShotData.Strength > 0 && Input.IsActionJustReleased(ShotData.Inverse ? InverseShootAction : ShootAction);
    }

    private bool ShouldCancelShot()
    {
        return ShotData.Inverse switch
        {
            true when Input.IsActionJustPressed(ShootAction) => true,
            false when Input.IsActionJustPressed(InverseShootAction) => true,
            _ => ShotData.Strength == 0 && ShotReleased()
        };
    }

    private void MakeIdleIfSleeping()
    {
        if (State == BallState.Rolling && IsSleeping)
        {
            State = BallState.Idle;
        }
    }

    private void HandlePocketCollision(Pocket pocket)
    {
        LinearVelocity = Vector2.Zero;
        Rotation = 0;
        // TODO fix, does not teleport
        var transform = Transform;
        transform.Origin = _initialGlobalPosition;
        Transform = transform;
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