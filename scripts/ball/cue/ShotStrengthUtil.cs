using Godot;

public static class ShotStrengthUtil
{

    private const float MinVelocity = 25;
    private const float MaxVelocity = 1400;
    private const float MinCueOffset = 0;
    private const float MaxCueOffset = 64;

    public static float GetVelocityForStrength(float strength)
    {
        if (strength == 0)
        {
            return MaxVelocity;
        }
        
        return Mathf.Lerp(MinVelocity, MaxVelocity, strength);
    }

    public static float GetCueOffsetForStrength(float strength)
    {
        return Mathf.Lerp(MinCueOffset, MaxCueOffset, strength);
    }

    public static Vector2 GetVelocity(Vector2 origin, ShotData shotData)
    {
        var shotVector = (shotData.AimPosition - origin).Normalized();
        return shotVector * GetVelocityForStrength(shotData.Strength);
    }
    
}