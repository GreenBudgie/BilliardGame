using Godot;

public partial class Billiard : Node2D
{
    public override void _Ready()
    {
        var stickerX2 = GD.Load<PackedScene>("res://scenes/sticker/sticker_x2.tscn");
        var stickerPlus2 = GD.Load<PackedScene>("res://scenes/sticker/sticker_plus_2.tscn");
        foreach (var pocket in PocketManager.Instance.GetPockets())
        {
            var sticker1 = stickerX2.Instantiate<Sticker>();
            pocket.StickerPositions[0].SetSticker(sticker1);
            var sticker2 = stickerPlus2.Instantiate<Sticker>();
            pocket.StickerPositions[1].SetSticker(sticker2);
        }
    }
}