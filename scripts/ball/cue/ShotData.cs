using Godot;

public record ShotData(
    // Pull vector, from the ball to the mouse cursor
    Vector2 PullVector,
    // Whether the user is using a RMB inverse shot
    bool Inverse,
    // A value (1-8) that indicates the shot strength
    int Strength
);