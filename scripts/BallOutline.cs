using Godot;
using System;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class BallOutline : RigidBody2D
{
    public override void _Ready()
    {
        ApplyImpulse(Vector2.Right * 100);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        state.LinearVelocity *= (1 - state.Step * 0.9f);
    }
    
}
