using Godot;

public record struct CollisionData(
    CollisionObject2D Collider,
    Vector2 InitialBallPosition,
    Vector2 InitialColliderPosition,
    Vector2 Normal,
    Vector2 InitialColliderVelocity,
    Vector2 CollisionPoint
);