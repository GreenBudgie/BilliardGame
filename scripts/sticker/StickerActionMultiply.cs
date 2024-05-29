using Godot;

public partial class StickerActionMultiply : StickerAction
{

    [Export] private float _times;
    
    public override void Trigger(PocketScoreContext context)
    {
        context.Score *= _times;
        GD.Print($"Triggered: {context.Score}");
    }

}