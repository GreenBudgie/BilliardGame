using Godot;

public partial class ScoringManager : Node
{
    
    public override void _Ready()
    {
        EventBus.Instance.BallStopped += HandleBallStop;
    }

    private void HandleBallStop(Ball ball)
    {
        var allBallsStopped = GameManager.Game.Table.GetBalls().TrueForAll(b => b.Sleeping);
        if (allBallsStopped)
        {
            EndShot();
        }
    }

    private async void EndShot()
    {
        
    }
    
}