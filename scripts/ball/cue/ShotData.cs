using Godot;

public record ShotData(
    // Non-normalized vector (direction and force) that will be applied when the ball is hit
    Vector2 Vector,
    // Whether the user is using a RMB inverse shot
    bool Inverse,
    // A value (1-8) that indicates the shot strength
    int Strength
)
{
    public static readonly float MinStrength = 1;
    public static readonly float MaxStrength = 8;
}