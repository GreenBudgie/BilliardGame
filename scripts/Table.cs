using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Table : Node2D
{
    
    public List<Pocket> Pockets { get; private set; }
    public List<PocketBall> PocketBalls { get; private set; }
    public CueBall CueBall { get; private set; }
    public StickerManager StickerManager { get; private set; }
    
    public override void _Ready()
    {
        Pockets = GetNode("Pockets").GetChildren().Cast<Pocket>().ToList();
        PocketBalls = GetNode("BallRack").GetChildren().Cast<PocketBall>().ToList();
        CueBall = GetNode<CueBall>("CueBall");
        StickerManager = GetNode<StickerManager>("StickerManager");
    }
}