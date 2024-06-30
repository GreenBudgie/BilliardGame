using System;
using Godot;

public partial class TrajectoryDrawer : Node2D
{
    [Export] private CueBall _cueBall;

    public override void _Ready()
    {
        if (_cueBall != null)
        {
            return;
        }

        GD.PrintErr("Cue ball is not assigned for the trajectory drawer");
        QueueFree();
    }

    public override void _Process(double delta)
    {
        Position = _cueBall.Position;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_cueBall.State != CueBall.BallState.ShotPrepare)
        {
            return;
        }

        if (_cueBall.ShotData.Strength == 0)
        {
            return;
        }

        Vector2 stopPoint;
        Vector2? collisionVector = null;
        if (_cueBall.ShapeCast.IsColliding())
        {
            var collisionPoint = _cueBall.ShapeCast.GetCollisionPoint(0);
            collisionVector = collisionPoint - GlobalPosition;
            stopPoint = GetBallCenterCollisionPoint(
                _cueBall.ShotData.PullVector,
                collisionVector.Value,
                _cueBall.Radius
            );
            
        }
        else
        {
            var travelDistance = ShotStrengthUtil.GetMaxTravelDistanceByStrength(_cueBall, _cueBall.ShotData.Strength);
            stopPoint = _cueBall.ShotData.PullVector.Normalized() * travelDistance;
        }

        DrawLine(Vector2.Zero, stopPoint, Colors.White, 1);
        DrawArc(stopPoint, _cueBall.Radius, 0, Mathf.Tau, 64, Colors.White, 1.5f);

        var angle = Mathf.Pi / 4f * _cueBall.ShotData.Strength;
        DrawArc(Vector2.Zero, _cueBall.Radius, 0, angle, 64, Colors.Orange, 1.5f);

        if (collisionVector.HasValue)
        {
            DrawCircle(collisionVector.Value, 2, Colors.Red);
        }
    }

    private Vector2 GetBallCenterCollisionPoint(Vector2 shootVector, Vector2 collisionPoint, float radius)
    {
        var a = shootVector.X;
        var b = shootVector.Y;
        var c = collisionPoint.X;
        var d = collisionPoint.Y;
        var sqrt = Mathf.Sqrt(
            Sq(-(2 * b * d) / a - 2 * c) - 4 * (Sq(b) / Sq(a) + 1) * (Sq(c) + Sq(d) - Sq(radius))
        );
        var component1 = 2 * b * d / a + 2 * c;
        var component2 = 2 * (Sq(b) / Sq(a) + 1);
        var x1 = (sqrt + component1) / component2;
        var x2 = (-sqrt + component1) / component2;
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