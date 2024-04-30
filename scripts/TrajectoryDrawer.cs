using Godot;

public partial class TrajectoryDrawer : Node2D
{
    [Export] private CueBall _cueBall;

    public override void _Ready()
    {
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the trajectory drawer");
            QueueFree();
            return;
        }
    }

    public override void _Process(double delta)
    {
        Position = _cueBall.Position;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_cueBall.IsShooting)
        {
            return;
        }

        if (!_cueBall.ShapeCast.IsColliding())
        {
            return;
        }

        var collisionPoint = _cueBall.ShapeCast.GetCollisionPoint(0);
        var collisionVector = collisionPoint - Position;
        var collisionVectorLength = collisionVector.Length();
        var closestPoint = GetClosestPoint(_cueBall.ShootVector.Normalized(), collisionVector, _cueBall.Radius);
        DrawLine(Vector2.Zero, closestPoint, Colors.White, 2, true);
        DrawArc(closestPoint, _cueBall.Radius, 0, Mathf.Tau, 64, Colors.White, 2f, true);
        DrawCircle(collisionVector, 4, Colors.Red);
    }

    private Vector2 GetClosestPoint(Vector2 line, Vector2 point, float radius)
    {
        var a = line.X;
        var b = line.Y;
        var c = point.X;
        var d = point.Y;
        var r = radius;
        var sqrt = Mathf.Sqrt(Sq(-(2 * b * d) / a - 2 * c) - 4 * (Sq(b) / Sq(a) + 1) *
            (Sq(c) + Sq(d) - Sq(r)));
        var x1 = (sqrt + (2 * b * d) / a + 2 * c) / (2 * (Sq(b) / Sq(a) + 1));
        var x2 = (-sqrt + (2 * b * d) / a + 2 * c) / (2 * (Sq(b) / Sq(a) + 1));
        var y1 = b * x1 / a;
        var y2 = b * x2 / a;
        var vector1 = new Vector2(x1, y1);
        var vector2 = new Vector2(x2, y2);
        var vector1LengthSq = vector1.LengthSquared();
        var vector2LengthSq = vector2.LengthSquared();
        return vector1LengthSq < vector2LengthSq ? vector1 : vector2;
    }

    private static float Sq(float number)
    {
        return number * number;
    }
}