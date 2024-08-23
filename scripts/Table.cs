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

}