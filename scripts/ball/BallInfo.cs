using Godot;

[GlobalClass]
public partial class BallInfo : Resource
{
    [Export] public bool IsCueBall { get; set; }
    [Export] public int Number { get; set; }
    [Export] public BallColor Color { get; set; }
    [Export] public BallType Type { get; set; }

    public BallInfo() : this(false, 0, BallColor.Yellow, BallType.Solid)
    {
    }

    public BallInfo(bool isCueBall, int number, BallColor color, BallType type)
    {
        IsCueBall = isCueBall;
        Number = number;
        Color = color;
        Type = type;
    }
}