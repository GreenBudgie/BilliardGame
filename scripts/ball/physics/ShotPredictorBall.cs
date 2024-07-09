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
        while (!IsSleeping && _predictedCollisions.Count < MaxCollisions)
        {
            var result = BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);
            var shotPrediction = ProcessPhysicsDependingOnLimitedTrajectoryLength(
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

        var lastTrajectory = GlobalPosition - lastPoint;
        if (_predictedCollisions.Count == 0)
        {
            if (lastTrajectory.Length() <= maxLength)
                return new ShotPrediction(GlobalPosition, _predictedCollisions);
            return new ShotPrediction(
                lastPoint + lastTrajectory.Normalized() * maxLength,
                _predictedCollisions
            );
        }

        var needToReduceLastTrajectoryLength = lastTrajectory.Length() + currentLength > maxLength;
        if (!needToReduceLastTrajectoryLength)
            return new ShotPrediction(GlobalPosition, _predictedCollisions);

        var stopPointWithReducedLength = lastPoint + lastTrajectory.Normalized() * (maxLength - currentLength);
        return new ShotPrediction(stopPointWithReducedLength, _predictedCollisions);
    }


    private ShotPrediction? ProcessPhysicsDependingOnLimitedTrajectoryLength(
        Dictionary<BallRigidBody, FullCollisionData> physicsResult,
        ref Vector2 lastPoint,
        float maxLength,
        ref float currentLength
    )
    {
        if (!physicsResult.ContainsKey(this)) return null;
        var newPoint = physicsResult[this].ContactPoint;
        var pointsDistance = (newPoint - lastPoint).Length();
        if (currentLength + pointsDistance > maxLength)
        {
            var newGlobalPosition = lastPoint + ((newPoint - lastPoint).Normalized() * (maxLength - currentLength));
            return new ShotPrediction(newGlobalPosition, _predictedCollisions);
        }

        _predictedCollisions.Add(physicsResult[this]);
        currentLength += pointsDistance;
        lastPoint = newPoint;
        return null;
    }


    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<FullCollisionData> Collisions
    );
}