using Godot;
using System;

public partial class CueBall : RigidBody2D
{

    [Export] public float ShotStrength = 10;
    
    private bool _isShooting;
    
    public override void _Process(double delta)
    {
        if (!_isShooting && Input.IsActionJustPressed("shoot"))
        {
            _isShooting = true;
        }

        if (_isShooting && Input.IsActionJustReleased("shoot"))
        {
            var shootVector = Position - GetGlobalMousePosition();
            ApplyCentralForce(shootVector * ShotStrength);
            _isShooting = false;
        }
    }
    
}
