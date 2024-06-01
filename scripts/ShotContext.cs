using System.Collections.Generic;
using Godot;

public partial class ShotContext : Node
{
    
    private readonly Dictionary<Pocket, List<Ball>> _scoredBalls = new();

    public override void _Ready()
    {
        EventBus.Instance.BallStopped += HandleBallStop;
    }

    public void AddScoredBall(Pocket pocket, Ball ball)
    {
        if (!_scoredBalls.ContainsKey(pocket))
        {
            _scoredBalls.Add(pocket, new List<Ball>());
        }
        _scoredBalls[pocket].Add(ball);
    }

    public List<Ball> GetBallsScoredToPocket(Pocket pocket)
    {
        return _scoredBalls.GetValueOrDefault(pocket, new List<Ball>());
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

    private void Reset()
    {
        _scoredBalls.Clear();
    }

}