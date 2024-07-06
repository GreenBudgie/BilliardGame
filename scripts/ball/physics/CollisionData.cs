using Godot;

public record struct CollisionData(
    CollisionObject2D Collider,
    Vector2 ColliderPosition,
    Vector2 ColliderVelocity,
    
    Vector2 BallPosition,
    
    Vector2 Normal,
    Vector2 CollisionPoint
);