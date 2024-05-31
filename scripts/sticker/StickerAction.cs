using Godot;

public abstract partial class StickerAction : Node
{
    
    protected Sticker Sticker { get; private set; }
    
    public override void _Ready()
    {
        Sticker = GetParent<Sticker>();
    }

    public abstract void Trigger(PocketScoreContext context);

}