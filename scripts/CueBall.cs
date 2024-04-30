using Godot;

public partial class CueBall : RigidBody2D
{
    [Export] public float ShotStrength = 10;

    public float Radius { get; private set; }
    private Vector2 _collisionPoint;

    public bool IsBallHovered { get; private set; }
    public bool IsShooting { get; private set; }
    public Vector2 ShootVector { get; private set; }
    public ShapeCast2D ShapeCast { get; private set; }

    public override void _Ready()
    {
        ShapeCast = GetNode<ShapeCast2D>("ShapeCast2D");

        var circleShape = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
        Radius = circleShape.Radius;
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
        ShootVector = Position - mousePosition;
        
        if (Input.IsActionJustReleased("shoot"))
        {
            ApplyCentralForce(ShootVector * ShotStrength);
            IsShooting = false;
        }
    }

}