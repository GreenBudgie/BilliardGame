using Godot;

public partial class ShootTrajectoryDrawer : Node2D
{
    
    public override void _Process(double delta)
    {
        Rotation = GetParent<Node2D>().Rotation;
        QueueRedraw();
    }

    public override void _Draw()
    {
        
    }
    
}
