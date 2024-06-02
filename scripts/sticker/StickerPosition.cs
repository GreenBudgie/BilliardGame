using Godot;

public partial class StickerPosition : Node2D
{
    
    private const string StickerNodeName = "Sticker";
    
    [Export]
    public Pocket Pocket { get; set; }

    public override void _Ready()
    {
        GetNode<Sprite2D>("Sprite2D").Visible = false;
    }

    public void SetSticker(Sticker sticker)
    {
        sticker.Name = StickerNodeName;
        sticker.Pocket = Pocket;
        AddChild(sticker);
    }

    public Sticker GetSticker()
    {
        return GetNodeOrNull<Sticker>(StickerNodeName);
    }
    
    public bool HasSticker()
    {
        return GetSticker() != null;
    }

}