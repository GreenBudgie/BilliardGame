using Godot;

public partial class CueBall : Ball
{
    private const string ShootAction = "shoot";
    private const string InverseShootAction = "inverse_shoot";

    [Signal]
    public delegate void ShotInitializedEventHandler();

    private Vector2 _initialGlobalPosition;

    public float Radius { get; private set; }

    public BallState State { get; private set; }

    public bool IsBallHovered { get; private set; }

    public ShotData ShotData { get; private set; } = new(Vector2.Zero, false, 0);

    public ShapeCast2D ShapeCast { get; private set; }

    private Font _font;

    public override void _Ready()
    {
        base._Ready();
        ShapeCast = GetNode<ShapeCast2D>("ShapeCast2D");

        var circleShape = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
        Radius = circleShape.Radius;

        _font = GD.Load<Font>("res://The Citadels.otf");

        _initialGlobalPosition = Transform.Origin;

        PocketScored += HandlePocketCollision;
        SleepingStateChanged += MakeIdleIfSleeping;
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        IsBallHovered = mousePosition.DistanceSquaredTo(Position) <= Radius * Radius;

        if (ShouldCancelShot())
        {
            State = BallState.Idle;
        }

        var ableToShoot = State == BallState.ShotPrepare || IsBallHovered;

        if (!ableToShoot)
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
        var pullVector = (mousePosition - Position) * inverseShotSign;

        var strength = ShotStrengthUtil.GetStrengthForPullVectorLength(pullVector.Length());

        ShotData = ShotData with
        {
            PullVector = pullVector,
            Strength = strength
        };

        if (strength > 0)
        {
            var travelDistance = ShotStrengthUtil.GetBallTravelDistanceForStrength(strength);
            ShapeCast.TargetPosition = GetLocalMousePosition().Normalized() * travelDistance * inverseShotSign;
            ShapeCast.ForceShapecastUpdate();
        }

        if (ShouldPerformShot())
        {
            State = BallState.ShotAnimation;
            EmitSignal(SignalName.ShotInitialized);
        }
    }

    public void PerformShot()
    {
        var velocity = ShotStrengthUtil.GetVelocityForStrength(ShotData.Strength);
        ApplyCentralForce(ShotData.PullVector.Normalized() * velocity);
        State = BallState.Rolling;
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
        if (State == BallState.Rolling && Sleeping)
        {
            State = BallState.Idle;
        }
    }

    private void HandlePocketCollision(Pocket pocket)
    {
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0;
        Rotation = 0;
        // TODO fix, does not teleport
        var transform = Transform;
        transform.Origin = _initialGlobalPosition;
        EventBus.Instance.EmitSignal(EventBus.SignalName.CueBallScored, this, pocket);
    }

    public enum BallState
    {
        Idle,
        Rolling,
        ShotPrepare,
        ShotAnimation
    }
}