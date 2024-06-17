using Godot;

public partial class NewPocketBall : Node2D
{

    private Sprite2D _ballSprite;
    private Sprite2D _overlaySprite;
    private SubViewport _viewport;

    public override void _Ready()
    {
        _ballSprite = GetNode<Sprite2D>("BallSprite");
        _overlaySprite = GetNode<Sprite2D>("OverlaySprite");
        _viewport = GetNode<SubViewport>("SubViewport");
        
    }

}
