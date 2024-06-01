using Godot;

public partial class BallInfo : Resource
{

    [Export] public bool IsCueBall { get; set; }
    [Export] public int Number { get; set; }
    [Export] public BallColor Color { get; set; }
    [Export] public BallType Type { get; set; }

    public override Rid _GetRid()
    {
        return base._GetRid();
    }
}