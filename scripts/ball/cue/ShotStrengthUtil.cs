using System.Collections.Generic;
using Godot;

public static class ShotStrengthUtil
{
    private const int MaxStrength = 8;

    private const float MinPullVectorLength = 20;
    private const float MaxPullVectorLength = 170;
    private static readonly float StrengthStep = (MaxPullVectorLength - MinPullVectorLength) / MaxStrength;

    private static readonly Dictionary<int, float> ImpulseForStrength = new()
    {
        { 1, 25 },
        { 2, 75 },
        { 3, 150 },
        { 4, 275 },
        { 5, 450 },
        { 6, 650 },
        { 7, 900 },
        { 8, 1200 },
    };

    private static readonly float CueOffsetPerStrength = 8;

    public static float GetImpulseForStrength(int strength)
    {
        return ImpulseForStrength[strength];
    }

    public static float GetCueOffsetForStrength(int strength)
    {
        return CueOffsetPerStrength * strength;
    }

    public static float GetMaxTravelDistanceByStrength(Ball body, int strength)
    {
        return GetMaxTravelDistanceByImpulse(body, ImpulseForStrength[strength]);
    }

    public static float GetMaxTravelDistanceByImpulse(Ball body, float impulse)
    {
        var initialVelocity = impulse;
        var spaceRid = PhysicsServer2D.BodyGetSpace(body.GetRid());
        var sleepThreshold = PhysicsServer2D.SpaceGetParam(
            spaceRid,
            PhysicsServer2D.SpaceParameter.BodyLinearVelocitySleepThreshold
        );
        var defaultLinearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var linearDamp = defaultLinearDamp; //body.LinearDamp == 0 ? defaultLinearDamp : body.LinearDamp;
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

    // TODO combine this method with above to improve performance
    public static float GetVelocityByTravelDistance(RigidBody2D body, float travelDistance, float impulse)
    {
        var initialVelocity = impulse / body.Mass;
        var spaceRid = PhysicsServer2D.BodyGetSpace(body.GetRid());
        var defaultLinearDamp = ProjectSettings.GetSetting("physics/2d/default_linear_damp").As<float>();
        var linearDamp = body.LinearDamp == 0 ? defaultLinearDamp : body.LinearDamp;
        var tps = Engine.PhysicsTicksPerSecond;
        var delta = 1d / tps;

        var currentVelocity = (double)initialVelocity;
        var currentTravelDistance = 0d;
        while (currentTravelDistance < travelDistance)
        {
            currentTravelDistance += currentVelocity;
            currentVelocity *= 1 - delta * linearDamp;
        }

        return (float)currentVelocity;
    }

    public static int GetStrengthForPullVectorLength(float length)
    {
        return Mathf.Clamp(Mathf.FloorToInt((length - MinPullVectorLength) / StrengthStep), 0, MaxStrength);
    }

    
}