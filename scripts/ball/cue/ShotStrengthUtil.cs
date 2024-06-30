using System.Collections.Generic;
using Godot;

public static class ShotStrengthUtil
{
    private const float MaxBallTravelDistance = 560;

    private const int MaxStrength = 8;

    private const float MinPullVectorLength = 20;
    private const float MaxPullVectorLength = 170;
    private static readonly float StrengthStep = (MaxPullVectorLength - MinPullVectorLength) / MaxStrength;

    private static readonly Dictionary<int, float> ImpulseForStrength = new()
    {
        { 1, 5 },
        { 2, 15 },
        { 3, 30 },
        { 4, 55 },
        { 5, 90 },
        { 6, 130 },
        { 7, 180 },
        { 8, 240 },
    };

    private static readonly float CueOffsetPerStrength = 8;

    public static float GetVelocityForStrength(int strength)
    {
        return ImpulseForStrength[strength];
    }

    public static float GetCueOffsetForStrength(int strength)
    {
        return CueOffsetPerStrength * strength;
    }

    public static float GetMaxTravelDistanceByStrength(RigidBody2D body, int strength)
    {
        return GetMaxTravelDistanceByImpulse(body, ImpulseForStrength[strength]);
    }

    public static float GetMaxTravelDistanceByImpulse(RigidBody2D body, float impulse)
    {
        var initialVelocity = impulse / body.Mass;
        var spaceRid = PhysicsServer2D.BodyGetSpace(body.GetRid());
        var sleepThreshold = PhysicsServer2D.SpaceGetParam(
            spaceRid,
            PhysicsServer2D.SpaceParameter.BodyLinearVelocitySleepThreshold
        );
        var linearDamp = body.LinearDamp;
        var tps = Engine.PhysicsTicksPerSecond;
        var delta = 1d / tps;
        
        var currentVelocity = (double)initialVelocity;
        var travelDistance = 0d;
        while (currentVelocity > sleepThreshold)
        {
            travelDistance += currentVelocity;
            currentVelocity *= 1 - delta * linearDamp;
        }

        return (float)(travelDistance / tps);
    }

    public static int GetStrengthForPullVectorLength(float length)
    {
        return Mathf.Clamp(Mathf.FloorToInt((length - MinPullVectorLength) / StrengthStep), 0, MaxStrength);
    }
}