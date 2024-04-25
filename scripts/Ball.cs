using System;
using Godot;

public partial class Ball : RigidBody2D
{

    [Export(PropertyHint.Range, "1,99")] public int Number { get; set; }

    [Export] public BallType Type { get; set; }

    [Export] public BallColor Color { get; set; }

    public override void _Ready()
    {
        var sprite = GetNode<Sprite2D>(Type.GetSpriteNodeName());
        sprite.Visible = true;
        sprite.Modulate = Color.GetRealColor();
        
        var numberLabel = GetNode<Label>("NumberLabel");
        numberLabel.Text = Number.ToString();
    }
    
}