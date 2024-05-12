using System;
using Godot;

public enum BallColor
{
    
    Yellow,
    Blue,
    Red,
    Purple,
    Orange,
    Green,
    Maroon,
    Black
    
}

public static class BallColorExtension
{

    public static Color GetRealColor(this BallColor ballColor)
    {
        return ballColor switch
        {
            BallColor.Yellow => new Color(0.95f, 0.9f, 0.1f),
            BallColor.Blue => new Color(0.2f, 0.2f, 0.7f),
            BallColor.Red => new Color(0.8f, 0.1f, 0.1f),
            BallColor.Purple => new Color(0.6f, 0.1f, 0.6f),
            BallColor.Orange => new Color(0.9f, 0.5f, 0.2f),
            BallColor.Green => new Color(0.1f, 0.6f, 0.1f),
            BallColor.Maroon => new Color(0.5f, 0.2f, 0.2f),
            BallColor.Black => new Color(0.1f, 0.1f, 0.1f),
            _ => throw new ArgumentOutOfRangeException(nameof(ballColor), ballColor, null)
        };
    }

}
