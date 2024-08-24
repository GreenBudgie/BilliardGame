using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ScoringManager : Node
{
    public static ScoringManager Instance { get; private set; }

    [Signal]
    public delegate void ScoreChangedEventHandler(int score);

    [Signal]
    public delegate void ScoringEndedEventHandler();

    public int Score { get; private set; }

    public int RequiredScore { get; private set; } = 20;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        BallManager.Instance.AllBallsStopped += HandleBallsStop;
    }

    public void IncreaseScore(float amount)
    {
        Score += Mathf.CeilToInt(amount);
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    private void HandleBallsStop()
    {
        EndShotExecution();
    }

    private async void EndShotExecution()
    {
        var contexts = new List<PocketScoreContext>();
        foreach (var pocket in PocketManager.Instance.GetPockets())
        {
            var context = await pocket.TriggerScoring();
            if (context != null)
            {
                contexts.Add(context);
            }
        }

        var scoreSum = contexts.Sum(context => context.Score);
        IncreaseScore(scoreSum);

        EmitSignal(SignalName.ScoringEnded);
    }
}