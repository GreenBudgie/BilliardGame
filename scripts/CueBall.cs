using Godot;

public partial class CueBall : Ball
{
    [Export] public float BaseShootStrengthMultiplier = 200;
    [Export] public float ShootStartThreshold = 6000;
    [Export] public float MinShootStrength = 500;
    [Export] public float MaxShootStrength = 85000;

    private static readonly string SHOOT_ACTION = "shoot";
    private static readonly string INVERSE_SHOOT_ACTION = "inverse_shoot";
    
    private Vector2 _initialGlobalPosition;

    public float Radius { get; private set; }
    
    public BallState State { get; private set; }

    public bool IsBallHovered { get; private set; }

    public bool IsInverseShot { get; private set; }

    // Vector that the ball is going to be hit, always normalized
    public Vector2 ShootVector { get; private set; }
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

        if (State == BallState.Idle && IsBallHovered && ShotPressed())
        {
            State = BallState.ShotPrepare;
            IsInverseShot = Input.IsActionJustPressed(INVERSE_SHOOT_ACTION);
            return;
        }

        if (State != BallState.ShotPrepare)
        {
            return;
        }

        var inverseShotSign = IsInverseShot ? -1 : 1;
        ShootVector = (mousePosition - Position) * BaseShootStrengthMultiplier * inverseShotSign;
        
        ShapeCast.TargetPosition = GetLocalMousePosition().Normalized() * 1500 * inverseShotSign;
        ShapeCast.ForceShapecastUpdate();

        if (ShootVector.LengthSquared() < ShootStartThreshold * ShootStartThreshold)
        {
            ShootVector = Vector2.Zero;
        }
        else
        {
            ShootVector -= ShootVector.Normalized() * ShootStartThreshold;
        }
        var shootVectorLengthSq = ShootVector.LengthSquared();
        if (shootVectorLengthSq < MinShootStrength * MinShootStrength)
        {
            ShootVector = ShootVector.Normalized() * MinShootStrength;
        }
        else if (shootVectorLengthSq > MaxShootStrength * MaxShootStrength)
        {
            ShootVector = ShootVector.Normalized() * MaxShootStrength;
        }

        if (ShouldPerformShot())
        {
            ApplyCentralForce(ShootVector);
            State = BallState.Rolling;
        }
    }
    
    private bool ShotPressed()
    {
        return Input.IsActionJustPressed(SHOOT_ACTION) || Input.IsActionJustPressed(INVERSE_SHOOT_ACTION);
    }
    
    private bool ShotReleased()
    {
        return Input.IsActionJustReleased(SHOOT_ACTION) || Input.IsActionJustReleased(INVERSE_SHOOT_ACTION);
    }

    private bool ShouldPerformShot()
    {
        return Input.IsActionJustReleased(IsInverseShot ? INVERSE_SHOOT_ACTION : SHOOT_ACTION);
    }

    private bool ShouldCancelShot()
    {
        return IsInverseShot switch
        {
            true when Input.IsActionJustPressed(SHOOT_ACTION) => true,
            false when Input.IsActionJustPressed(INVERSE_SHOOT_ACTION) => true,
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
        
        Idle, Rolling, ShotPrepare
        
    }

}