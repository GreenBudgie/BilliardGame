using Godot;

public abstract partial class StickerAction : Node2D
{
    
    public Sticker Sticker { get; set; }

    public abstract void Trigger(PocketScoreContext context);

}