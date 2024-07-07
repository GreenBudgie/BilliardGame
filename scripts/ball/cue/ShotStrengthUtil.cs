using Godot;

public static class ShotStrengthUtil
{

    private const float MinVelocity = 25;
    private const float MaxVelocity = 1400;
    private const float MinPullVectorLength = 20;
    private const float MaxPullVectorLength = 170;
    private const float MinCueOffset = 0;
    private const float MaxCueOffset = 64;

    public static float GetVelocityForPullVectorLength(float length)
    {
        if (length < MinPullVectorLength)
        {
            return 0;
        }

        var clampedLength = Mathf.Clamp(length, MinPullVectorLength, MaxPullVectorLength);
        var weight = (clampedLength - MinPullVectorLength) / (MaxPullVectorLength - MinPullVectorLength);
        
        return Mathf.Lerp(MinVelocity, MaxVelocity, weight);
    }

    public static float GetCueOffsetForVelocity(float velocity)
    {
        var weight = (velocity - MinVelocity) / (MaxVelocity - MinVelocity);
        return Mathf.Lerp(MinCueOffset, MaxCueOffset, weight);
    }

    
}