using System;
using System.Collections.Generic;
using Godot;

public static class ShotPredictor
{
    private const int MaxCollisions = 10;

    public static ShotPrediction GetShotPrediction(
        CueBall cueBall,
        CharacterBody2D collisionTester,
        Vector2 initialVelocity
    )
    {
        var spaceRid = PhysicsServer2D.BodyGetSpace(cueBall.GetRid());
        var sleepThreshold = PhysicsServer2D.SpaceGetParam(
            spaceRid,
            PhysicsServer2D.SpaceParameter.BodyLinearVelocitySleepThreshold
        );
        var sleepThresholdSq = sleepThreshold * sleepThreshold;
        var defaultLinearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var linearDamp = defaultLinearDamp; //cueBall.LinearDamp == 0 ? defaultLinearDamp : cueBall.LinearDamp;
        var tps = (float)Engine.PhysicsTicksPerSecond;
        var delta = 1d / tps;

        var currentVelocity = initialVelocity;
        var currentPosition = cueBall.GlobalPosition;
        List<CollisionPrediction> collisions = new();
        while (currentVelocity.LengthSquared() > sleepThresholdSq && collisions.Count < MaxCollisions)
        {
            var realVelocity = currentVelocity / tps;
            collisionTester.GlobalPosition = currentPosition;
            var collision = collisionTester.MoveAndCollide(realVelocity, true);
            if (collision == null)
            {
                currentPosition += realVelocity;
                currentVelocity *= (float)(1 - delta * linearDamp);
                continue;
            }
            currentPosition += collision.GetTravel();
            CollisionPrediction prediction;
            if (collision.GetCollider() is PocketBall pocketBall)
            {
                prediction = PredictBallCollision(
                    currentPosition,
                    pocketBall.GlobalPosition,
                    currentVelocity,
                    collision
                );
            }
            else
            {
                prediction = PredictBorderCollision(
                    currentPosition,
                    currentVelocity,
                    collision
                );
            }

            collisions.Add(prediction);
            currentVelocity = prediction.CueBallCollision.VelocityAfterContact * (float)(1 - delta * linearDamp);
        }

        return new ShotPrediction(currentPosition, collisions.ToArray());
    }

    private static CollisionPrediction PredictBallCollision(
        Vector2 cueBallPosition,
        Vector2 pocketBallPosition,
        Vector2 currentVelocity,
        KinematicCollision2D collision
    )
    {
        var bodyVector = cueBallPosition - pocketBallPosition;
        var resultVelocity = currentVelocity.Dot(bodyVector) / bodyVector.LengthSquared() * bodyVector;
        var cueBallVelocity = currentVelocity - resultVelocity;
        var cueBallPrediction = new BallCollisionPrediction(
            cueBallPosition,
            currentVelocity,
            cueBallVelocity
        );
        var pocketBallPrediction = new BallCollisionPrediction(
            pocketBallPosition,
            Vector2.Zero,
            resultVelocity
        );
        return new CollisionPrediction(
            collision.GetPosition(),
            collision.GetNormal(),
            cueBallPrediction,
            pocketBallPrediction
        );
    }

    private static CollisionPrediction PredictBorderCollision(
        Vector2 cueBallPosition,
        Vector2 currentVelocity,
        KinematicCollision2D collision
    )
    {
        var normal = collision.GetNormal();
        var newVelocity = currentVelocity.Bounce(normal);
        var ballCollisionPrediction = new BallCollisionPrediction(
            cueBallPosition,
            currentVelocity,
            newVelocity
        );
        return new CollisionPrediction(
            collision.GetPosition(),
            normal,
            ballCollisionPrediction,
            null
        );
    }

    public record struct ShotPrediction(
        Vector2 StopPoint,
        CollisionPrediction[] Collisions
    );

    public record struct CollisionPrediction(
        Vector2 ContactPoint,
        Vector2 Normal,
        BallCollisionPrediction CueBallCollision,
        BallCollisionPrediction? OtherBallCollision
    );

    public record struct BallCollisionPrediction(
        Vector2 Position,
        Vector2 VelocityBeforeContact,
        Vector2 VelocityAfterContact
    );
}