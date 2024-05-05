using Billiard.scripts;
using Godot;

public partial class CueBall : Ball
{
    [Export] public float BaseShootStrengthMultiplier = 200;
    [Export] public float ShootStartThreshold = 6000;
    [Export] public float MinShootStrength = 500;
    [Export] public float MaxShootStrength = 85000;

    private Vector2 _initialGlobalPosition;

    public float Radius { get; private set; }

    public bool IsBallHovered { get; private set; }

    public bool IsShooting { get; private set; }

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
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        IsBallHovered = mousePosition.DistanceSquaredTo(Position) <= Radius * Radius;

        if (IsBallHovered && Input.IsActionJustReleased("shoot"))
        {
            IsShooting = false;
            Input.SetDefaultCursorShape();
            return;
        }

        var ableToShoot = IsShooting || IsBallHovered;

        if (!ableToShoot)
        {
            Input.SetDefaultCursorShape();
            return;
        }

        Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);

        if (!IsShooting && IsBallHovered && Input.IsActionJustPressed("shoot"))
        {
            IsShooting = true;
            return;
        }

        if (!IsShooting)
        {
            return;
        }

        ShapeCast.TargetPosition = -GetLocalMousePosition().Normalized() * 1500;
        ShapeCast.ForceShapecastUpdate();
        
        ShootVector = (Position - mousePosition) * BaseShootStrengthMultiplier;
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

        if (Input.IsActionJustReleased("shoot"))
        {
            ApplyCentralForce(ShootVector);
            IsShooting = false;
        }
        
        QueueRedraw();
    }

    public override void _Draw()
    {
        //DrawString(_font, new Vector2(0, -70), ShootVector.Length().ToString(), HorizontalAlignment.Center, -1, 32);
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
    
}