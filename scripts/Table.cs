using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Table : Node2D
{
	
	public List<Pocket> Pockets { get; private set; }

	public override void _Ready()
	{
		Pockets = GetNode("Pockets").GetChildren().Cast<Pocket>().ToList();
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
}
