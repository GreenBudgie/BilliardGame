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
        foreach (var collision in shotPrediction.Collisions)
        {
            var currentPosition = collision.BallData.Position - GlobalPosition;
            var contactPoint = collision.ContactPoint - GlobalPosition;
            DrawLine(previousPoint, currentPosition, Colors.White, 1);
            DrawArc(currentPosition, _cueBall.Radius, 0, Mathf.Tau, 16, Colors.White, 1.5f);
            DrawCircle(collision.ContactPoint - GlobalPosition, 2, Colors.Red);
            DrawLine(contactPoint, contactPoint + collision.Normal * 100, Colors.Orange, 1);
            if (collision.OtherBallData.HasValue)
            {
                var pocketBallVelocity = collision.OtherBallData.Value.VelocityAfterContact;
                var pocketBallPosition = collision.OtherBallData.Value.Position - GlobalPosition;
                DrawLine(
                    pocketBallPosition,
                    pocketBallPosition + pocketBallVelocity.Normalized() * 256,
                    Colors.Aqua,
                    1
                );
            }

            previousPoint = collision.BallData.Position - GlobalPosition;
        }

        DrawLine(previousPoint, stopPoint, Colors.White, 1);
    }

}