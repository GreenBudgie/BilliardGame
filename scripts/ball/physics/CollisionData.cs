using Godot;

public readonly record struct CollisionData(
    CollisionObject2D Collider,
    Vector2 Position,
    Vector2 ColliderPosition,
    Vector2 Normal,
    Vector2 ColliderVelocity,
    Vector2 CollisionPoint
);