using Godot;
using System;

public partial class BallOutline : Sprite2D
{
    public override void _Process(double delta)
    {
        RotationDegrees += (float)delta * 20;
    }
}
