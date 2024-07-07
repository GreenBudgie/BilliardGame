using Godot;

public record struct FullCollisionData(
    Vector2 ContactPoint,
    Vector2 Normal,
    BallCollisionData BallData,
    BallCollisionData? OtherBallData
);

public record struct BallCollisionData(
    Vector2 Position,
    Vector2 VelocityBeforeContact,
    Vector2 VelocityAfterContact
);