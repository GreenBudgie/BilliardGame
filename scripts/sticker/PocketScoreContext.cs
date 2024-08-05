using Godot;

public partial class PocketScoreContext : RefCounted
{

    public float Score { get; set; }

    public PocketScoreContext(float initialScore)
    {
        Score = initialScore;
    }

    public PocketScoreContext()
    {
    }
}