using System.Collections.Generic;
using Godot;

public partial class TrajectoryDrawer : Node2D
{
    [Export] private CueBall _cueBall;

    private ShotPredictorBall _shotPredictorBall;
    private Line2D _trajectory;

    private readonly List<Line2D> _trajectories = new();

    private ShotPredictorBall.ShotPrediction? _lastShotPrediction;
    
    private const float StepsToPredictIncrease = 450;
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

        if (_cueBall.ShotData.Strength == 0)
        {
            ResetPrediction();
            return;
        }
        
        _stepsToPredict += Mathf.Min(StepsToPredictIncrease * (float)delta, MaxStepsToPredict);
        
        var impulse = ShotStrengthUtil.GetImpulseForStrength(_cueBall.ShotData.Strength);
        var initialVelocity = _cueBall.ShotData.PullVector.Normalized() * impulse;
        _shotPredictorBall.GlobalPosition = GlobalPosition;
        _lastShotPrediction = _shotPredictorBall.GetShotPrediction(
            initialVelocity,
            Mathf.RoundToInt(_stepsToPredict)
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
            var contactPoint = collision.ContactPoint - GlobalPosition;
            DrawArc(currentPosition, _cueBall.Radius, 0, Mathf.Tau, 16, Colors.White, 1.5f);
            DrawCircle(collision.ContactPoint - GlobalPosition, 2, Colors.Red);
            DrawLine(contactPoint, contactPoint + collision.Normal * 32, Colors.Orange, 1);
            if (!collision.OtherBallData.HasValue)
            {
                continue;
            }
            
            var pocketBallVelocity = collision.OtherBallData.Value.VelocityAfterContact;
            var pocketBallPosition = collision.OtherBallData.Value.Position - GlobalPosition;
            DrawLine(
                pocketBallPosition,
                pocketBallPosition + pocketBallVelocity.Normalized() * 64,
                Colors.Aqua,
                1
            );
        }
    }

    private void ResetPrediction()
    {
        _stepsToPredict = 0;
        _lastShotPrediction = null;
        QueueRedraw();
    }
}