using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BallManager : Node
{
    [Signal]
    public delegate void BallScoredEventHandler(Ball ball, Pocket pocket);
    
    [Signal]
    public delegate void BallStoppedEventHandler(Ball ball);
    
    [Signal]
    public delegate void AllBallsStoppedEventHandler();

    public static BallManager Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }
    
    public List<PocketBall> GetPocketBalls()
    {
        return GetNode("BallRack").GetChildren().Cast<PocketBall>().ToList();
    }
    
    public CueBall GetCueBall()
    {
        return GetNode<CueBall>("CueBall");
    }

    public List<Ball> GetBalls()
    {
        return GetPocketBalls().Concat<Ball>(new[] { GetCueBall() }).ToList();
    }
    
    public void HandleBallStopped(Ball ball)
    {
        EmitSignal(SignalName.BallStopped, ball);
        
        var allBallsStopped = GetBalls().TrueForAll(b => b.Sleeping);
        if (allBallsStopped)
        {
            EmitSignal(SignalName.AllBallsStopped);
        }
    }
    
}