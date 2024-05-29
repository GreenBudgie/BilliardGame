using Godot;

public partial class ScoreCounter : CounterLabel
{

    public override void _Ready()
    {
        EventBus.Instance.ScoringEnded += HandleScoringEnd;
    }

    private void HandleScoringEnd(PocketScoreContext context)
    {
        Count += Mathf.RoundToInt(context.Score);
    }

}