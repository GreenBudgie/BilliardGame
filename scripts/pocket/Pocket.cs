using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Pocket : StaticBody2D
{

    [Export] public PocketPosition PocketPosition { get; private set; }
    
    public List<StickerPosition> StickerPositions { get; private set; }

    public List<BallInfo> ScoredBalls { get; private set; } = new();
    public List<BallInfo> ScoredBallsPerShot { get; private set; } = new();
    
    public override void _Ready()
    {
        var stickerPositionsNode = GetNode<Node2D>("StickerPositions");
        StickerPositions = stickerPositionsNode.GetChildren().Cast<StickerPosition>().ToList();
        foreach (var stickerPosition in StickerPositions)
        {
            stickerPosition.Visible = false;
            stickerPosition.Pocket = this;
        }

        EventBus.Instance.BallScored += HandleBallScore;
    }

    public async Task TriggerScoring()
    {
        var initialScore = ScoredBallsPerShot.Sum(ball => ball.Number);
        var context = new PocketScoreContext(initialScore);
        var stickers = GameManager.Game.Table.StickerManager.GetStickersForPocket(this);
        foreach (var sticker in stickers)
        {
            await sticker.Trigger(context);
        }
    }
    
    private void HandleBallScore(Ball ball, Pocket pocket)
    {
        if (pocket != this)
        {
            return;
        }
        ScoredBalls.Add(ball.BallInfo);
        ScoredBallsPerShot.Add(ball.BallInfo);
    }

}
