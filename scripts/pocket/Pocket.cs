using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Pocket : Node2D
{

    [Export] public PocketPosition PocketPosition { get; private set; }
    
    public List<StickerPosition> StickerPositions { get; private set; }

    public List<BallInfo> ScoredBalls { get; } = new();
    public List<BallInfo> ScoredBallsPerShot { get; } = new();
    
    public override void _Ready()
    {
        var stickerPositionsNode = GetNode<Node2D>("StickerPositions");
        StickerPositions = stickerPositionsNode.GetChildren().Cast<StickerPosition>().ToList();

        BallManager.Instance.BallScored += _HandleBallScore;
    }

    public List<StickerPosition> GetEmptyPositions()
    {
        return StickerPositions.Where(stickerPosition => !stickerPosition.HasSticker()).ToList();
    }

    public List<Sticker> GetStickers()
    {
        return StickerPositions
            .Select(stickerPosition => stickerPosition.GetSticker())
            .Where(sticker => sticker != null)
            .ToList();
    }

    public async ValueTask<PocketScoreContext> TriggerScoring()
    {
        if (ScoredBallsPerShot.Count == 0)
        {
            return null;
        }
        var initialScore = ScoredBallsPerShot.Sum(ball => ball.Number);
        var context = new PocketScoreContext(initialScore);
        foreach (var sticker in GetStickers())
        {
            await sticker.Trigger(context);
        }
        ScoredBallsPerShot.Clear();

        return context;
    }
    
    private void _HandleBallScore(Ball ball, Pocket pocket)
    {
        if (pocket != this)
        {
            return;
        }
        ScoredBalls.Add(ball.BallInfo);
        ScoredBallsPerShot.Add(ball.BallInfo);
    }

}
