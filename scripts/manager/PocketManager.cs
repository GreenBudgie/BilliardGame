using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PocketManager : Node2D
{
    
    public static PocketManager Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public List<Pocket> GetPockets()
    {
        return GetChildren().Cast<Pocket>().ToList();
    }
    
}