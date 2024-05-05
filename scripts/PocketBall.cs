using System.Linq;
using Billiard.scripts;
using Godot;

public partial class PocketBall : Ball
{

    [Export(PropertyHint.Range, "1,99")] public int Number { get; set; } = 1;

    [Export] public BallType Type { get; set; }

    [Export] public BallColor Color { get; set; }

    public override void _Ready()
    {
        base._Ready();
        var parts = GetNode("Parts");
        
        // Make all sprites invisible at first
        var sprites = parts.GetChildren().OfType<Sprite2D>();
        foreach (var sprite in sprites)
        {
            sprite.Visible = false;
        }
        
        // Make appropriate sprites for the ball type visible and apply ball color to overlay
        var bodySprite = parts.GetNode<Sprite2D>(Type.GetBodySpriteNodeName());
        bodySprite.Visible = true;
        var overlaySprite = parts.GetNode<Sprite2D>(Type.GetOverlaySpriteNodeName());
        overlaySprite.Visible = true;
        overlaySprite.Modulate = Color.GetRealColor();

        var numberLabel = GetNode<Label>("NumberLabel");
        numberLabel.Text = Number.ToString();

        PocketScored += HandlePocketCollision;
    }

    private void HandlePocketCollision(Pocket pocket)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.PocketScored, this, pocket);
        QueueFree();
    }
    
}