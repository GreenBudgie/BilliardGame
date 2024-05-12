using System.Collections.Generic;
using Godot;

public static class ShotStrengthUtil
{

    public static readonly int MaxStrength = 8;

    private static readonly float MinPullVectorLength = 64;
    private static readonly float MaxPullVectorLength = 336;
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
    
    public static float GetVelocityForStrength(int strength)
    {
        return VelocityForStrength[strength];
    }
    
    public static float GetCueOffsetForStrength(int strength)
    {
        return CueOffsetPerStrength * strength;
    }

    public static int GetStrengthForPullVectorLength(float length)
    {
        return Mathf.Clamp(Mathf.FloorToInt((length - MinPullVectorLength) / StrengthStep), 0, MaxStrength);
    }

}