using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pocket : StaticBody2D
{

    public List<StickerPosition> StickerPositions { get; private set; }
    
    public override void _Ready()
    {
        var stickerPositionsNode = GetNode<Node2D>("StickerPositions");
        StickerPositions = stickerPositionsNode.GetChildren().Cast<StickerPosition>().ToList();
        foreach (var stickerPosition in StickerPositions)
        {
            stickerPosition.Visible = false;
            stickerPosition.Pocket = this;
        }
    }

}
