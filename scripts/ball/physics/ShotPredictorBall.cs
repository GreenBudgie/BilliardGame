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
        var initialVelocityLength = initialVelocity.Length();
        // Limit initial velocity by maxLength 
        if (initialVelocityLength > maxLength)
            initialVelocity = initialVelocity / initialVelocityLength * maxLength;
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


    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<FullCollisionData> Collisions
    );
}