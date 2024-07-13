using System.Collections.Generic;
using Godot;

public partial class TrajectoryDrawer : Node2D
{
    [Export] private CueBall _cueBall;

    private ShotPredictorBall _shotPredictorBall;
    private Line2D _trajectory;

    private readonly List<Line2D> _trajectories = new();

    private ShotPredictorBall.ShotPrediction? _lastShotPrediction;

    private const float StepsToPredictIncrease = 420;
    private const float MaxStepsToPredict = 10000;
    private float _stepsToPredict;

    public override void _Ready()
    {
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the trajectory drawer");
            QueueFree();
            return;
        }

        _shotPredictorBall = GetNode<ShotPredictorBall>("ShotPredictorBall");
        _trajectory = GetNode<Line2D>("Trajectory");

        _trajectories.Add(_trajectory);
        for (var i = 0; i < ShotPredictorBall.MaxCollisions; i++)
        {
            var newTrajectory = (Line2D)_trajectory.Duplicate();
            _trajectories.Add(newTrajectory);
            AddChild(newTrajectory);
        }
    }

    public override void _Process(double delta)
    {
        Position = _cueBall.Position;

        foreach (var trajectory in _trajectories)
        {
            trajectory.ClearPoints();
        }

        if (_cueBall.State != CueBall.BallState.ShotPrepare)
        {
            ResetPrediction();
            return;
        }

        if (_cueBall.ShotData.Velocity == 0)
        {
            ResetPrediction();
            return;
        }

        _stepsToPredict += Mathf.Min(StepsToPredictIncrease * (float)delta, MaxStepsToPredict);

        var initialVelocity = _cueBall.ShotData.PullVector.Normalized() * _cueBall.ShotData.Velocity;
        _shotPredictorBall.GlobalPosition = GlobalPosition;
        _lastShotPrediction = _shotPredictorBall.GetShotPredictionWithLimitedTrajectoryLength(
            initialVelocity,
            Mathf.RoundToInt(_stepsToPredict),
            300
        );
        var shotPrediction = _lastShotPrediction.Value;
        var stopPoint = shotPrediction.StopPoint - GlobalPosition;
        var previousPoint = _cueBall.GlobalPosition - GlobalPosition;
        for (var i = 0; i < shotPrediction.Collisions.Count; i++)
        {
            var trajectory = _trajectories[i];
            var collision = shotPrediction.Collisions[i];
            var currentPosition = collision.BallData.Position - GlobalPosition;
            trajectory.AddPoint(previousPoint);
            trajectory.AddPoint(currentPosition);
            previousPoint = collision.BallData.Position - GlobalPosition;
        }

        var finalTrajectory = _trajectories[shotPrediction.Collisions.Count];
        finalTrajectory.AddPoint(previousPoint);
        finalTrajectory.AddPoint(stopPoint);

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_lastShotPrediction.HasValue)
        {
            return;
        }

        var shotPrediction = _lastShotPrediction.Value;
        foreach (var collision in shotPrediction.Collisions)
        {
            var currentPosition = collision.BallData.Position - GlobalPosition;
            var collisionCoordinates = collision.ContactPoint - GlobalPosition;
            var newBallPositionGap = (currentPosition - collisionCoordinates).Normalized() * _cueBall.Radius;
            var newBallPosition = collisionCoordinates + newBallPositionGap;
            // Draw a ball on collision spot
            DrawArc(newBallPosition, _cueBall.Radius, 0, Mathf.Tau, 16, Colors.White, 1.5f);
            // Draw a dot where collision will happen 
            DrawCircle(collisionCoordinates, 2, Colors.Red);
            if (!collision.OtherBallData.HasValue)
            {
                continue;
            }

            var pocketBallVelocity = collision.OtherBallData.Value.VelocityAfterContact;
            var pocketBallPosition = collision.OtherBallData.Value.Position - GlobalPosition;
            var velocityVector = pocketBallVelocity.Normalized() * 8 + pocketBallVelocity * 0.05f;
            DrawLine(
                pocketBallPosition,
                pocketBallPosition + velocityVector,
                Colors.Orange,
                1.5f
            );
        }

        // Draw a dot on ball stop point
        DrawCircle(shotPrediction.StopPoint - GlobalPosition, 2, Colors.Orange);
    }

    private void ResetPrediction()
    {
        _stepsToPredict = 0;
        _lastShotPrediction = null;
        QueueRedraw();
    }
}