using Godot;

public readonly record struct CollisionStatus(
    CollisionObject2D Body,
    Vector2 InitialCollisionPoint
);