using Godot;

public partial class PocketScoreContext : RefCounted
{
    
    public Ball ScoredBall { get; private set; }
    public Pocket Pocket { get; private set; }
    public float Score { get; set; }

    public PocketScoreContext(Ball scoredBall, Pocket pocket, float initialScore)
    {
        ScoredBall = scoredBall;
        Pocket = pocket;
        Score = initialScore;
    }

}