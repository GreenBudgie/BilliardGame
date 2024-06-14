using Godot;
using System;

public partial class TestBall : Sprite2D
{
    private Font _font;

    public override void _Ready()
    {
        _font = GD.Load<Font>("res://resources/fonts/font_baloo.ttf");
        RenderingServer.FramePostDraw += HandleFramePostDraw;
    }

    private void HandleFramePostDraw()
    {
        var subViewport = GetNode<SubViewport>("../SubViewportContainer/SubViewport");
        var texture = subViewport.GetTexture();
        var shaderMaterial = (ShaderMaterial)Material;
        shaderMaterial.SetShaderParameter("ball_texture", texture);
    }

    public override void _Draw()
    {
        DrawString(_font, Position, "T", fontSize: 64, modulate: Colors.Aqua);
    }
}