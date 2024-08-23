using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BallManager : Node
{
    [Signal]
    public delegate void BallStoppedEventHandler(Ball ball);
    
    [Signal]
    public delegate void AllBallsStoppedEventHandler();

    public static BallManager Instance;

    public override void _Ready()
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