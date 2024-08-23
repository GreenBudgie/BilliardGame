using System;
using Godot;

public partial class PocketBall : Ball
{
    private static readonly Texture2D SolidBallMask = GD.Load<Texture2D>("res://sprites/ball/solid_ball_mask.png");
    private static readonly Texture2D StripeBallMask = GD.Load<Texture2D>("res://sprites/ball/stripe_ball_mask.png");

    private Sprite2D _ballSprite;
    private Sprite2D _overlaySprite;

    public override void _Ready()
    {
        base._Ready();

        _ballSprite = GetNode<Sprite2D>("BallSprite");
        _overlaySprite = GetNode<Sprite2D>("OverlaySprite");
        _ballSprite.Texture = BallInfo.Type switch
        {
            BallType.Solid => SolidBallMask,
            BallType.Stripe => StripeBallMask,
            _ => throw new ArgumentOutOfRangeException()
        };

        var ballSpriteMaterial = (ShaderMaterial)_ballSprite.Material;
        ballSpriteMaterial.SetShaderParameter("paint_color", BallInfo.Color.GetRealColor());

        var ballNumber = GetNode<Label>("SubViewport/CenterContainer/BallNumber");
        ballNumber.Text = BallInfo?.Number.ToString();

        PocketScored += _HandlePocketCollision;
    }

    protected override void RotateSprites(Vector4 finalRotation)
    {
        var ballSpriteMaterial = (ShaderMaterial)_ballSprite.Material;
        var overlaySpriteMaterial = (ShaderMaterial)_overlaySprite.Material;
        ballSpriteMaterial.SetShaderParameter("rotation", finalRotation);
        overlaySpriteMaterial.SetShaderParameter("rotation", finalRotation);
    }

    private void _HandlePocketCollision(Pocket pocket)
    {
        BallManager.Instance.EmitSignal(BallManager.SignalName.BallScored, this, pocket);
        QueueFree();
    }
}