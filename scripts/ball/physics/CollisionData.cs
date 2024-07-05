using Godot;

public readonly record struct CollisionData(
    CollisionObject2D Collider,
    Vector2 Normal,
    Vector2 ColliderVelocity
);