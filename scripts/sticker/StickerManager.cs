using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class StickerManager : Node2D
{
    public override void _Ready()
    {
        EventBus.Instance.BallScored += HandlePocketScore;
    }

    public void AddSticker(Sticker sticker, StickerPosition position)
    {
        AddChild(sticker);
        sticker.GlobalPosition = position.GlobalPosition;
        sticker.Pocket = position.Pocket;
        
    }

    public List<Sticker> GetStickers()
    {
        return GetChildren().Cast<Sticker>().ToList();
    }

    public List<Sticker> GetStickersForPocket(Pocket pocket)
    {
        return GetStickers().Where(sticker => sticker.Pocket == pocket).ToList();
    }

    private async void HandlePocketScore(Ball ball, Pocket pocket)
    {
        var initialScore = ball is PocketBall pocketBall ? pocketBall.Number : 0;
        var pocketScoreContext = new PocketScoreContext(ball, pocket, initialScore);
        var stickers = GetStickersForPocket(pocket);
        foreach (var sticker in stickers)
        {
            await sticker.Trigger(pocketScoreContext);
        }

        EventBus.Instance.EmitSignal(EventBus.SignalName.ScoringEnded, pocketScoreContext);
    }
    
}