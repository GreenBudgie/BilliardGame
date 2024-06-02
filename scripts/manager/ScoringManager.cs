using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ScoringManager : Node
{

    public int Score { get; private set; }
    
    public override void _Ready()
    {
        EventBus.Instance.BallStopped += HandleBallStop;
    }

    public void IncreaseScore(float amount)
    {
        Score += Mathf.CeilToInt(amount);
        EventBus.Instance.EmitSignal(EventBus.SignalName.ScoreChanged, Score);
    }

    private void HandleBallStop(Ball ball)
    {
        if (GameManager.Game.GameStateManager.State != GameState.ShotExecution)
        {
            return;
        }
        var allBallsStopped = GameManager.Game.Table.GetBalls().TrueForAll(b => b.Sleeping);
        if (allBallsStopped)
        {
            EndShotExecution();
        }
    }

    private async void EndShotExecution()
    {
        GameManager.Game.GameStateManager.ChangeState(GameState.ScoreCalculation);
        var contexts = new List<PocketScoreContext>();
        foreach (var pocket in GameManager.Game.Table.Pockets)
        {
            var context = await pocket.TriggerScoring();
            if (context != null)
            {
                contexts.Add(context);
            }
        }

        var scoreSum = contexts.Sum(context => context.Score);
        IncreaseScore(scoreSum);

        GameManager.Game.GameStateManager.ChangeState(GameState.ShotPreparation);
    }

}