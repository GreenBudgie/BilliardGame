using Godot;
using System;

public partial class BallOutline : RigidBody2D
{
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        state.ApplyForce(Vector2.Right * 100);
    }
    
}
