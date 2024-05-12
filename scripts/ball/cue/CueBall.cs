using Godot;

public partial class CueBall : Ball
{
    [Export] public float BaseShootStrengthMultiplier = 200;
    [Export] public float ShootStartThreshold = 6000;
    [Export] public float MinShootStrength = 500;
    [Export] public float MaxShootStrength = 85000;

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
        var shotVector = (mousePosition - Position) * BaseShootStrengthMultiplier * inverseShotSign;

        ShapeCast.TargetPosition = GetLocalMousePosition().Normalized() * 1500 * inverseShotSign;
        ShapeCast.ForceShapecastUpdate();

        if (shotVector.LengthSquared() < ShootStartThreshold * ShootStartThreshold)
        {
            shotVector = Vector2.Zero;
        }
        else
        {
            shotVector -= shotVector.Normalized() * ShootStartThreshold;
        }
        var shootVectorLengthSq = shotVector.LengthSquared();
        if (shootVectorLengthSq < MinShootStrength * MinShootStrength)
        {
            shotVector = shotVector.Normalized() * MinShootStrength;
        }
        else if (shootVectorLengthSq > MaxShootStrength * MaxShootStrength)
        {
            shotVector = shotVector.Normalized() * MaxShootStrength;
        }

        ShotData = ShotData with { Vector = shotVector };

        if (ShouldPerformShot())
        {
            State = BallState.ShotAnimation;
            EmitSignal(SignalName.ShotInitialized);
        }
    }

    public void PerformShot()
    {
        ApplyCentralForce(ShotData.Vector);
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
        return Input.IsActionJustReleased(ShotData.Inverse ? InverseShootAction : ShootAction);
    }

    private bool ShouldCancelShot()
    {
        return ShotData.Inverse switch
        {
            true when Input.IsActionJustPressed(ShootAction) => true,
            false when Input.IsActionJustPressed(InverseShootAction) => true,
            _ => IsBallHovered && ShotReleased()
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
        var transform = Transform;
        transform.Origin = _initialGlobalPosition;
        EventBus.Instance.EmitSignal(EventBus.SignalName.CueBallScored, this, pocket);
    }

    public enum BallState
    {
        
        Idle, Rolling, ShotPrepare, ShotAnimation
        
    }

}