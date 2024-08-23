using Godot;

public partial class Billiard : Node2D
{
    public Table Table { get; private set; }
    public ScoringManager ScoringManager { get; private set; }
    public GameStateManager GameStateManager { get; private set; }

    public override void _Ready()
    {
        Table = GetNode<Table>("Table");
        ScoringManager = GetNode<ScoringManager>("Managers/ScoringManager");
        GameStateManager = GetNode<GameStateManager>("Managers/GameStateManager");
        var stickerX2 = GD.Load<PackedScene>("res://scenes/sticker/sticker_x2.tscn");
        foreach (var pocket in Table.Pockets)
        {
            var sticker1 = stickerX2.Instantiate<Sticker>();
            pocket.StickerPositions[0].SetSticker(sticker1);
            var sticker2 = stickerX2.Instantiate<Sticker>();
            pocket.StickerPositions[1].SetSticker(sticker2);
        }
    }
}