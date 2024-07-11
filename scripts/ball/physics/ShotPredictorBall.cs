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

    private readonly List<FullCollisionData> _predictedCollisions = new();

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
        ShotPrediction? shotPrediction;
        float lastTrajectoryLength;
        while (!IsSleeping && _predictedCollisions.Count < MaxCollisions)
        {
            var result = BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);

            shotPrediction = ProcessPhysic(lastPoint, maxLength, currentLength, out lastTrajectoryLength);
            // if we're already beyond the limit it will not be necessary to process next collisions 
            if (shotPrediction != null)
                return shotPrediction.Value;

            if (result.ContainsKey(this))
            {
                // Process a collision
                shotPrediction = ProcessPhysic(
                    lastPoint,
                    maxLength,
                    currentLength,
                    out lastTrajectoryLength
                );
                lastPoint = GlobalPosition;
                currentLength += lastTrajectoryLength;
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

        shotPrediction = ProcessPhysic(lastPoint, maxLength, currentLength, out lastTrajectoryLength);
        return shotPrediction ?? new ShotPrediction(GlobalPosition, _predictedCollisions);
    }

    private ShotPrediction? ProcessPhysic(
        Vector2 lastPoint,
        float maxLength,
        float currentLength,
        out float lastTrajectoryLength
    )
    {
        var lastTrajectory = GlobalPosition - lastPoint;
        lastTrajectoryLength = lastTrajectory.Length();
        var currentPredictionLength = currentLength + lastTrajectoryLength;
        if (currentPredictionLength < maxLength) return null;
        
        var stopPointWithReducedLength =
            lastPoint + lastTrajectory / lastTrajectoryLength * (maxLength - currentLength);
        return new ShotPrediction(stopPointWithReducedLength, _predictedCollisions);
    }


    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<FullCollisionData> Collisions
    );
}