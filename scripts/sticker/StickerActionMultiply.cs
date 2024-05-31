using Godot;

public partial class StickerActionMultiply : StickerAction
{

    [Export] private float _times;
    
    public override void Trigger(PocketScoreContext context)
    {
        context.Score *= _times;
        var effectLabel = EffectLabel.Create();
        effectLabel.Position += Vector2.Up * 40;
        effectLabel.Text = $"x{_times}";
        Sticker.AddChild(effectLabel);
    }

}