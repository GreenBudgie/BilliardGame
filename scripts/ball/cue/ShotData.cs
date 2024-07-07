using Godot;

public record ShotData(
    // Pull vector, from the ball to the mouse cursor
    Vector2 PullVector,
    // Whether the user is using a RMB inverse shot
    bool Inverse,
    // Initial velocity of the cue ball when the shot is performed
    float Velocity
);