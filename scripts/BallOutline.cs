using Godot;
using System;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class BallOutline : CharacterBody2D
{
    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("shoot"))
        {
            var shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
            shapeCast.ForceShapecastUpdate();
            var collision = MoveAndCollide(Vector2.Zero, true);
            var collider = (Node2D)collision.GetCollider();
            GD.Print(collider.Name, collision.GetNormal());
        }
    }
}