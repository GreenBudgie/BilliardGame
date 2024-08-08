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

    private ShotData _shotData;

    public override void _Ready()
    {
        _shotPredictorBall = GetNode<ShotPredictorBall>("ShotPredictorBall");
        _trajectory = GetNode<Line2D>("Trajectory");

        _trajectories.Add(_trajectory);
        for (var i = 0; i < ShotPredictorBall.MaxCollisions; i++)
        {
            var newTrajectory = (Line2D)_trajectory.Duplicate();
            _trajectories.Add(newTrajectory);
            AddChild(newTrajectory);
        }
        
        EventBus.Instance.ShotDataChanged += _HandleShotDataChange;
        EventBus.Instance.ShotInitialized += _HandleShotInitialization;
    }

    public override void _Process(double delta)
    {
        if (_cueBall != null)
        {
            Position = _cueBall.Position;
        }
        
        if (_shotData == null)
        {
            ResetPrediction();
            return;
        }

        foreach (var trajectory in _trajectories)
        {
            trajectory.ClearPoints();
        }

        _stepsToPredict += Mathf.Min(StepsToPredictIncrease * (float)delta, MaxStepsToPredict);

        var initialVelocity = ShotStrengthUtil.GetVelocity(GlobalPosition, _shotData);
        _shotPredictorBall.GlobalPosition = GlobalPosition;
        _lastShotPrediction = _shotPredictorBall.GetShotPrediction(
            initialVelocity,
            (int)MaxStepsToPredict
        );
        var shotPrediction = _lastShotPrediction.Value;
        var stopPoint = shotPrediction.StopPoint - GlobalPosition;
        var previousPoint = Vector2.Zero;
        for (var i = 0; i < shotPrediction.Collisions.Count; i++)
        {
            var trajectory = _trajectories[i];
            var collision = shotPrediction.Collisions[i];
            var currentPosition = collision.GetPosition() - GlobalPosition;
            trajectory.AddPoint(previousPoint);
            trajectory.AddPoint(currentPosition);
            previousPoint = collision.GetPosition() - GlobalPosition;
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
            var currentPosition = collision.GetPosition() - GlobalPosition;
            var collisionCoordinates = collision.GetPosition() - GlobalPosition;
            var newBallPositionGap = (currentPosition - collisionCoordinates).Normalized() * _cueBall.Radius;
            var newBallPosition = collisionCoordinates + newBallPositionGap;
            // Draw a ball on collision spot
            DrawArc(currentPosition, _cueBall.Radius, 0, Mathf.Tau, 16, Colors.White, 1.5f);
            // Draw a dot where collision will happen 
            DrawCircle(collisionCoordinates, 2, Colors.Red);
            DrawLine(currentPosition, currentPosition + collision.GetNormal() * 30, Colors.Blue);
            if (collision.GetCollider() is not BallRigidBody otherBall)
            {
                continue;
            }

            var pocketBallVelocity = otherBall.LinearVelocity; //TODO Fix after
            var pocketBallPosition = otherBall.GlobalPosition - GlobalPosition;
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

    private void _HandleShotDataChange(ShotData newShotData)
    {
        _shotData = newShotData;
    }
    
    private void _HandleShotInitialization(ShotData newShotData)
    {
        _shotData = null;
    }
    
}