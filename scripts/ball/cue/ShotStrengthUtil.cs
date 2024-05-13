using System.Collections.Generic;
using Godot;

public static class ShotStrengthUtil
{
    private const float MaxBallTravelDistance = 1650;

    private const int MaxStrength = 8;

    private const float MinPullVectorLength = 64;
    private const float MaxPullVectorLength = 512;
    private static readonly float StrengthStep = (MaxPullVectorLength - MinPullVectorLength) / MaxStrength;

    private static readonly Dictionary<int, float> VelocityForStrength = new()
    {
        { 1, 4000 },
        { 2, 8000 },
        { 3, 15000 },
        { 4, 24000 },
        { 5, 35000 },
        { 6, 48000 },
        { 7, 63000 },
        { 8, 80000 },
    };

    private static readonly float CueOffsetPerStrength = 24;

    private static readonly Dictionary<int, float> BallTravelDistanceForStrength = new()
    {
        { 1, 94.5859f },
        { 2, 282.576f },
        { 3, 671.474f },
        { 4, 1171.49f }
    };

    public static float GetVelocityForStrength(int strength)
    {
        return VelocityForStrength[strength];
    }

    public static float GetCueOffsetForStrength(int strength)
    {
        return CueOffsetPerStrength * strength;
    }

    public static float GetBallTravelDistanceForStrength(int strength)
    {
        if (!BallTravelDistanceForStrength.ContainsKey(strength))
        {
            return MaxBallTravelDistance;
        }

        return BallTravelDistanceForStrength[strength];
    }

    public static int GetStrengthForPullVectorLength(float length)
    {
        return Mathf.Clamp(Mathf.FloorToInt((length - MinPullVectorLength) / StrengthStep), 0, MaxStrength);
    }
}