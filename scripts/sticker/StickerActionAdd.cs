using Godot;

public partial class StickerActionAdd : StickerAction
{

    [Export] private float _points;
    
    public override void Trigger(PocketScoreContext context)
    {
        context.Score += _points;
        var effectLabel = EffectLabel.Create();
        effectLabel.Position += Vector2.Up * 40;
        effectLabel.Text = $"+{_points}";
        Sticker.AddChild(effectLabel);
    }

}