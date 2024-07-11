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
        while (!IsSleeping && _predictedCollisions.Count < MaxCollisions)
        {
            var result = BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);
           
            shotPrediction = ProcessCurrentGlobalPosition(lastPoint, maxLength, currentLength);
            // if we're already beyond the limit it will not be necessary to process next collisions 
            if (shotPrediction != null)
                return shotPrediction.Value;
            
            shotPrediction = ProcessCollision(
                result,
                ref lastPoint,
                maxLength,
                ref currentLength
            );
            if (shotPrediction != null)
                return shotPrediction.Value;

            if (maxSteps > 0 && step >= maxSteps)
            {
                break;
            }

            step++;
        }
        shotPrediction = ProcessCurrentGlobalPosition(lastPoint, maxLength, currentLength);
        return shotPrediction ?? new ShotPrediction(GlobalPosition, _predictedCollisions);
    }


    private ShotPrediction? ProcessCollision(
        Dictionary<BallRigidBody, FullCollisionData> physicsResult,
        ref Vector2 lastPoint,
        float maxLength,
        ref float currentLength
    )
    {
        if (!physicsResult.ContainsKey(this)) return null;
        var newPoint = physicsResult[this].ContactPoint;
        var pointsVector = newPoint - lastPoint;
        var pointsVectorLength = pointsVector.Length();
        if (currentLength + pointsVectorLength > maxLength)
        {
            var newGlobalPosition = lastPoint + pointsVector/pointsVectorLength * (maxLength - currentLength);
            return new ShotPrediction(newGlobalPosition, _predictedCollisions);
        }

        _predictedCollisions.Add(physicsResult[this]);
        currentLength += pointsVectorLength;
        lastPoint = newPoint;
        return null;
    }
    
    private ShotPrediction? ProcessCurrentGlobalPosition(
        Vector2 lastPoint,
        float maxLength,
        float currentLength
    )
    {
        var lastTrajectory = GlobalPosition - lastPoint;
        var lastTrajectoryLength = lastTrajectory.Length();
        var currentPredictionLength = currentLength + lastTrajectoryLength;
        if (Math.Abs(currentPredictionLength - maxLength) < 0.0001)
        {
            return new ShotPrediction(GlobalPosition, _predictedCollisions);
        }
        if (!(currentPredictionLength > maxLength)) return null;

        var stopPointWithReducedLength = lastPoint + lastTrajectory/lastTrajectoryLength * (maxLength - currentLength);
        return new ShotPrediction(stopPointWithReducedLength, _predictedCollisions);
    }
    
    


    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<FullCollisionData> Collisions
    );
}