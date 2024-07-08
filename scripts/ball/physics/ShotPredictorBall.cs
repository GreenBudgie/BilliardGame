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
    private readonly List<FullCollisionData> _predictedCollisionsWithRestrictedLength = new();

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

    public ShotPrediction GetShotPredictionWithMaxTrajectoryLength(Vector2 initialVelocity, int maxSteps,
        Vector2 initialPosition, float maxLength)
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

        return RestrictShotPrediction(initialPosition, maxLength);
    }


    private ShotPrediction RestrictShotPrediction(Vector2 initialPosition, float maxLength)
    {
        _predictedCollisionsWithRestrictedLength.Clear();
        if (_predictedCollisions.Count == 0)
        {
            if ((GlobalPosition - initialPosition).Length() <= maxLength)
                return new ShotPrediction(GlobalPosition, _predictedCollisions);
            return new ShotPrediction(initialPosition + ((GlobalPosition - initialPosition).Normalized() * maxLength),
                _predictedCollisions);
        }

        var totalLength = 0f;
        var lastPoint = initialPosition;
        var newGlobalPosition = new Vector2();
        foreach (var collision in _predictedCollisions)
        {
            var newPoint = collision.ContactPoint;
            var pointsDistance = (newPoint - lastPoint).Length();
            if (totalLength + pointsDistance > maxLength)
            {
                newGlobalPosition = lastPoint + ((newPoint - lastPoint).Normalized() * (maxLength - totalLength));
                return new ShotPrediction(newGlobalPosition, _predictedCollisionsWithRestrictedLength);
            }

            _predictedCollisionsWithRestrictedLength.Add(collision);
            totalLength += pointsDistance;
            lastPoint = newPoint;
        }

        if ((GlobalPosition - _predictedCollisions[^1].ContactPoint).Length() +
            totalLength <= maxLength)
            return new ShotPrediction(GlobalPosition, _predictedCollisionsWithRestrictedLength);

        newGlobalPosition = lastPoint + ((GlobalPosition - lastPoint).Normalized() * (maxLength - totalLength));
        return new ShotPrediction(newGlobalPosition, _predictedCollisionsWithRestrictedLength);
    }

    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<FullCollisionData> Collisions
    );
}