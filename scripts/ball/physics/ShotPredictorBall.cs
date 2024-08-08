using System;
using System.Collections.Generic;
using Godot;

public partial class ShotPredictorBall : BallRigidBody
{
    public const int MaxCollisions = 10;
    private readonly List<BallRigidBody> _predictorBallList = new();

    public override void _Ready()
    {
        base._Ready();

        _predictorBallList.Add(this);
    }

    public override bool AwakesOtherBalls()
    {
        return false;
    }

    private readonly List<KinematicCollision2D> _predictedCollisions = new();

    public ShotPrediction GetShotPrediction(Vector2 initialVelocity, int maxSteps)
    {
        var step = 0;
        var delta = BallPhysicsServer.Instance.DefaultDelta;
        _predictedCollisions.Clear();
        SetLinearVelocity(initialVelocity);
        while (!IsSleeping && _predictedCollisions.Count < MaxCollisions)
        {
            var result = BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);
            if (result.ContainsKey(this))
            {
                _predictedCollisions.Add(result[this]);
            }

            if (maxSteps > 0 && step >= maxSteps)
            {
                break;
            }

            step++;
        }

        return new ShotPrediction(GlobalPosition, _predictedCollisions);
    }

    public ShotPrediction GetShotPredictionWithLimitedTrajectoryLength(Vector2 initialVelocity, int maxSteps,
        float maxLength)
    {
        var step = 0;
        var delta = BallPhysicsServer.Instance.DefaultDelta;
        _predictedCollisions.Clear();
        SetLinearVelocity(initialVelocity);
        var currentLength = 0f;
        var lastPoint = GlobalPosition;
        while (!IsSleeping && _predictedCollisions.Count < MaxCollisions)
        {
            var result = BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);

            var shotPrediction = ReduceTrajectoryLengthIfNeeded(ref lastPoint, maxLength, ref currentLength);

            if (result.ContainsKey(this))
            {
                _predictedCollisions.Add(result[this]);
            }

            if (shotPrediction != null)
                return shotPrediction.Value;

            if (maxSteps > 0 && step >= maxSteps)
            {
                break;
            }

            step++;
        }

        return new ShotPrediction(GlobalPosition, _predictedCollisions);
    }

    private ShotPrediction? ReduceTrajectoryLengthIfNeeded(
        ref Vector2 lastPoint,
        float maxLength,
        ref float currentLength
    )
    {
        var lastTrajectory = GlobalPosition - lastPoint;
        var lastTrajectoryLength = lastTrajectory.Length();
        var currentPredictionLength = currentLength + lastTrajectoryLength;
        if (currentPredictionLength < maxLength)
        {
            currentLength = currentPredictionLength;
            lastPoint = GlobalPosition;
            return null;
        }
        
        var stopPointWithReducedLength =
            lastPoint + lastTrajectory / lastTrajectoryLength * (maxLength - currentLength);
        return new ShotPrediction(stopPointWithReducedLength, _predictedCollisions);
    }


    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<KinematicCollision2D> Collisions
    );
}