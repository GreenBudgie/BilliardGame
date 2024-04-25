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
            BallColor.Yellow => Colors.Yellow,
            BallColor.Blue => Colors.Blue,
            BallColor.Red => Colors.Red,
            BallColor.Purple => Colors.Purple,
            BallColor.Orange => Colors.Orange,
            BallColor.Green => Colors.Green,
            BallColor.Maroon => Colors.Maroon,
            BallColor.Black => Colors.Black,
            _ => throw new ArgumentOutOfRangeException(nameof(ballColor), ballColor, null)
        };
    }

}
