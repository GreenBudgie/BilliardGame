using Godot;

public partial class Game : Node2D
{
    
    public Table Table { get; private set; }
    public ShotContext ShotContext { get; private set; }

    public override void _Ready()
    {
        Table = GetNode<Table>("Table");
        ShotContext = GetNode<ShotContext>("ShotContext");
        var stickerX2 = GD.Load<PackedScene>("res://scenes/sticker/sticker_x2.tscn");
        foreach (var pocket in Table.Pockets)
        {
            var sticker1 = stickerX2.Instantiate<Sticker>();
            Table.StickerManager.AddSticker(sticker1, pocket.StickerPositions[0]);
            var sticker2 = stickerX2.Instantiate<Sticker>();
            Table.StickerManager.AddSticker(sticker2, pocket.StickerPositions[1]);
        }
    }
}