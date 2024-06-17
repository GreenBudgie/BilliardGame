using System;
using Godot;

public partial class PocketBall : Ball
{
    private static readonly Texture2D SolidBallMask = GD.Load<Texture2D>("res://sprites/ball/solid_ball_mask.png");
    private static readonly Texture2D StripeBallMask = GD.Load<Texture2D>("res://sprites/ball/stripe_ball_mask.png");

    public override void _Ready()
    {
        base._Ready();

        var ballSprite = GetNode<Sprite2D>("BallSprite");
        ballSprite.Texture = BallInfo.Type switch
        {
            BallType.Solid => SolidBallMask,
            BallType.Stripe => StripeBallMask,
            _ => throw new ArgumentOutOfRangeException()
        };

        var ballSpriteMaterial = (ShaderMaterial)ballSprite.Material;
        ballSpriteMaterial.SetShaderParameter("paint_color", BallInfo.Color.GetRealColor());

        var ballNumber = GetNode<Label>("SubViewport/CenterContainer/BallNumber");
        ballNumber.Text = BallInfo?.Number.ToString();

        PocketScored += HandlePocketCollision;
    }

    private void HandlePocketCollision(Pocket pocket)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.BallScored, this, pocket);
        QueueFree();
    }
}