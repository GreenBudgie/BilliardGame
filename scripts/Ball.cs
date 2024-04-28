using System;
using System.Linq;
using Godot;

public partial class Ball : RigidBody2D
{

    [Export(PropertyHint.Range, "1,99")] public int Number { get; set; }

    [Export] public BallType Type { get; set; }

    [Export] public BallColor Color { get; set; }

    public override void _Ready()
    {
        // Make all sprites invisible at first
        var sprites = GetChildren().OfType<Sprite2D>();
        foreach (var sprite in sprites)
        {
            sprite.Visible = false;
        }
        
        // Make appropriate sprites for the ball type visible and apply ball color to overlay
        var bodySprite = GetNode<Sprite2D>(Type.GetBodySpriteNodeName());
        bodySprite.Visible = true;
        var overlaySprite = GetNode<Sprite2D>(Type.GetOverlaySpriteNodeName());
        overlaySprite.Visible = true;
        overlaySprite.Modulate = Color.GetRealColor();

        var numberLabel = GetNode<Label>("NumberLabel");
        numberLabel.Text = Number.ToString();
        if (Number > 9)
        {
            //numberLabel.LabelSettings.FontSize = 24;
        }
    }

}