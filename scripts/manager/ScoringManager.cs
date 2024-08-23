using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ScoringManager : Node
{

    public int Score { get; private set; }
    
    public override void _Ready()
    {
        BallManager.Instance.AllBallsStopped += HandleBallsStop;
    }

    public void IncreaseScore(float amount)
    {
        Score += Mathf.CeilToInt(amount);
        EventBus.Instance.EmitSignal(EventBus.SignalName.ScoreChanged, Score);
    }

    private void HandleBallsStop()
    {
        EndShotExecution();
    }

    private async void EndShotExecution()
    {
        BilliardManager.Billiard.GameStateManager.ChangeState(GameState.ScoreCalculation);
        var contexts = new List<PocketScoreContext>();
        foreach (var pocket in BilliardManager.Billiard.Table.Pockets)
        {
            var context = await pocket.TriggerScoring();
            if (context != null)
            {
                contexts.Add(context);
            }
        }

        var scoreSum = contexts.Sum(context => context.Score);
        IncreaseScore(scoreSum);

        BilliardManager.Billiard.GameStateManager.ChangeState(GameState.ShotPreparation);
    }

}