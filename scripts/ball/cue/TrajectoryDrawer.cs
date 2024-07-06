using System;
using Godot;

public partial class TrajectoryDrawer : Node2D
{
    [Export] private CueBall _cueBall;

    private ShotPredictorBall _shotPredictorBall;

    public override void _Ready()
    {
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the trajectory drawer");
            QueueFree();
            return;
        }

        _shotPredictorBall = GetNode<ShotPredictorBall>("ShotPredictorBall");
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

        var impulse = ShotStrengthUtil.GetImpulseForStrength(_cueBall.ShotData.Strength);
        var initialVelocity = _cueBall.ShotData.PullVector.Normalized() * impulse;
        _shotPredictorBall.GlobalPosition = GlobalPosition;
        var shotPrediction = _shotPredictorBall.GetShotPrediction(initialVelocity);
        var stopPoint = shotPrediction.StopPoint - GlobalPosition;
        DrawCircle(stopPoint, 2, Colors.White);
        var previousPoint = _cueBall.GlobalPosition - GlobalPosition;
        for (var i = 0; i < shotPrediction.Collisions.Count; i++)
        {
            var collision = shotPrediction.Collisions[i];
            var currentPosition = collision.BallPosition - GlobalPosition;
            var contactPoint = collision.CollisionPoint - GlobalPosition;
            DrawLine(previousPoint, currentPosition, Colors.White, 1);
            DrawArc(currentPosition, _cueBall.Radius, 0, Mathf.Tau, 16, Colors.White, 1.5f);
            DrawCircle(collision.CollisionPoint - GlobalPosition, 2, Colors.Red);
            DrawLine(contactPoint, contactPoint + collision.Normal * 100, Colors.Orange, 1);
            if (collision.IsBallCollision)
            {
                var pocketBallVelocity = collision.ColliderVelocity;
                var pocketBallPosition = collision.ColliderPosition - GlobalPosition;
                DrawLine(pocketBallPosition, pocketBallPosition + pocketBallVelocity.Normalized() * 256, Colors.Aqua,
                    1);
            }

            previousPoint = shotPrediction.Collisions[i].BallPosition - GlobalPosition;
        }

        DrawLine(previousPoint, stopPoint, Colors.White, 1);

        // Vector2 stopPoint;
        // Vector2? collisionPoint = null;
        // Vector2? bounceVector = null;
        // Vector2? collisionNormal = null;
        // if (_cueBall.ShapeCast.IsColliding())
        // {
        //     collisionPoint = _cueBall.ShapeCast.GetCollisionPoint(0) - GlobalPosition;
        //     stopPoint = GetBallCenterCollisionPoint(
        //         _cueBall.ShotData.PullVector,
        //         collisionPoint.Value,
        //         _cueBall.Radius
        //     );
        //     collisionNormal = _cueBall.ShapeCast.GetCollisionNormal(0);
        //     bounceVector = stopPoint.Bounce(collisionNormal.Value);
        // }
        // else
        // {
        //     var travelDistance = ShotStrengthUtil.GetMaxTravelDistanceByStrength(_cueBall, _cueBall.ShotData.Strength);
        //     stopPoint = _cueBall.ShotData.PullVector.Normalized() * travelDistance;
        // }
        //
        // DrawLine(Vector2.Zero, stopPoint, Colors.White, 1);
        // DrawArc(stopPoint, _cueBall.Radius, 0, Mathf.Tau, 64, Colors.White, 1.5f);
        //
        // var angle = Mathf.Pi / 4f * _cueBall.ShotData.Strength;
        // DrawArc(Vector2.Zero, _cueBall.Radius, 0, angle, 64, Colors.Orange, 1.5f);
        //
        // if (collisionPoint.HasValue)
        // {
        //     DrawCircle(collisionPoint.Value, 2, Colors.Red);
        //     DrawLine(stopPoint, stopPoint + bounceVector.Value, Colors.White, 1);
        //     DrawLine(stopPoint, stopPoint + collisionNormal.Value * 100, Colors.Aqua, 1);
        // }
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