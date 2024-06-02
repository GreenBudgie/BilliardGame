using System.Linq;
using Godot;

public partial class PocketBall : Ball
{

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
        var bodySprite = parts.GetNode<Sprite2D>(BallInfo.Type.GetBodySpriteNodeName());
        bodySprite.Visible = true;
        var overlaySprite = parts.GetNode<Sprite2D>(BallInfo.Type.GetOverlaySpriteNodeName());
        overlaySprite.Visible = true;
        overlaySprite.Modulate = BallInfo.Color.GetRealColor();

        var numberLabel = GetNode<Label>("NumberLabel");
        numberLabel.Text = BallInfo.Number.ToString();

        PocketScored += HandlePocketCollision;
    }

    private void HandlePocketCollision(Pocket pocket)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.BallScored, this, pocket);
        QueueFree();
    }
    
}