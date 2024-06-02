﻿using Godot;

public partial class StickerPosition : Node2D
{
    
    public Pocket Pocket { get; set; }

    public override void _Ready()
    {
        GetNode<Sprite2D>("Sprite2D").Visible = false;
    }

    public void SetSticker(Sticker sticker)
    {
        AddChild(sticker);
        sticker.Pocket = Pocket;
    }

    public Sticker GetSticker()
    {
        return GetChildOrNull<Sticker>(0);
    }
    
    public bool HasSticker()
    {
        return GetSticker() != null;
    }

}