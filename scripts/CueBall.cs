using Godot;

public partial class CueBall : RigidBody2D
{

    [Export] public float ShotStrength = 10;

    private ShapeCast2D _shapeCast;
    
    private bool _isShooting;
    private bool _ableToShoot;

    private float _radius;
    private Vector2 _collisionPoint;

    public override void _Ready()
    {
        _shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
        
        var circleShape = (CircleShape2D) GetNode<CollisionShape2D>("CollisionShape2D").Shape;
        _radius = circleShape.Radius;
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        _ableToShoot = _isShooting || mousePosition.DistanceSquaredTo(Position) <= _radius * _radius;
        
        if (!_ableToShoot)
        {
            Input.SetDefaultCursorShape();
            return;
        }
        Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);

        if (!_isShooting && Input.IsActionJustPressed("shoot"))
        {
            _isShooting = true;
            return;
        }

        if (!_isShooting)
        {
            return;
        }
        
        var shootVector = Position - mousePosition;

        _shapeCast.TargetPosition = shootVector.Normalized() * 1500;
        _shapeCast.ForceShapecastUpdate();

        if (Input.IsActionJustReleased("shoot"))
        {
            ApplyCentralForce(shootVector * ShotStrength);
            _isShooting = false;
        }
        
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_isShooting)
        {
            return;
        }

        if (!_shapeCast.IsColliding())
        {
            return;
        }
        
        var collisionPoint = _shapeCast.GetCollisionPoint(0);
        DrawLine(Vector2.Zero, collisionPoint - Position, Colors.White, 2, true);
        DrawCircle(collisionPoint - Position, 8, Colors.Red);
    }
    
}
