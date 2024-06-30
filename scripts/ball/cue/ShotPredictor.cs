using System;
using System.Collections.Generic;
using Godot;

public static class ShotPredictor
{
    public static ShotPrediction GetShotPrediction(CueBall cueBall, CharacterBody2D collisionTester,
        Vector2 initialVelocity)
    {
        var spaceRid = PhysicsServer2D.BodyGetSpace(cueBall.GetRid());
        var sleepThreshold = PhysicsServer2D.SpaceGetParam(
            spaceRid,
            PhysicsServer2D.SpaceParameter.BodyLinearVelocitySleepThreshold
        );
        var sleepThresholdSq = sleepThreshold * sleepThreshold;
        var defaultLinearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var linearDamp = cueBall.LinearDamp == 0 ? defaultLinearDamp : cueBall.LinearDamp;
        var tps = Engine.PhysicsTicksPerSecond;
        var delta = 1d / tps;

        var currentVelocity = initialVelocity;
        var currentPosition = cueBall.GlobalPosition;
        List<CollisionPrediction> collisions = new();
        while (currentVelocity.LengthSquared() > sleepThresholdSq)
        {
            var realVelocity = currentVelocity / tps;
            collisionTester.GlobalPosition = currentPosition;
            var collision = collisionTester.MoveAndCollide(realVelocity, true);
            if (collision != null)
            {
                currentPosition +=
                    collision.GetTravel(); // TODO Maybe need to also move by remainder by updated velocity?
                if (collision.GetCollider() is PocketBall pocketBall)
                {
                    var bodyVector = currentPosition - pocketBall.GlobalPosition;
                    var cueBallVelocity = currentVelocity -
                                          (currentVelocity.Dot(bodyVector) / bodyVector.LengthSquared()) * bodyVector;
                    var pocketBallVelocity =
                        -(-currentVelocity.Dot(-bodyVector) / -bodyVector.LengthSquared()) *
                        bodyVector; //TODO remove - if possible
                    var cueBallPrediction = new BallCollisionPrediction(
                        currentPosition,
                        currentVelocity,
                        cueBallVelocity
                    );
                    var pocketBallPrediction = new BallCollisionPrediction(
                        pocketBall.GlobalPosition,
                        currentVelocity,
                        pocketBallVelocity
                    );
                    collisions.Add(
                        new CollisionPrediction(
                            collision.GetPosition(),
                            cueBallPrediction,
                            pocketBallPrediction
                        )
                    );
                    currentVelocity = cueBallVelocity;
                    continue;
                }

                var normal = collision.GetNormal();
                var newVelocity = currentVelocity.Bounce(normal);
                var ballCollisionPrediction = new BallCollisionPrediction(
                    currentPosition,
                    currentVelocity,
                    newVelocity
                );
                collisions.Add(
                    new CollisionPrediction(
                        collision.GetPosition(),
                        ballCollisionPrediction,
                        null
                    )
                );
                currentVelocity = newVelocity;
                continue;
            }

            currentPosition += realVelocity;
            currentVelocity *= (float)(1 - delta * linearDamp);
        }

        return new ShotPrediction(currentPosition, collisions.ToArray());
    }

    public record struct ShotPrediction(
        Vector2 StopPoint,
        CollisionPrediction[] Collisions
    );

    public record struct CollisionPrediction(
        Vector2 ContactPoint,
        BallCollisionPrediction CueBallCollision,
        BallCollisionPrediction? OtherBallCollision
    );

    public record struct BallCollisionPrediction(
        Vector2 Position,
        Vector2 VelocityBeforeContact,
        Vector2 VelocityAfterContact
    );
}