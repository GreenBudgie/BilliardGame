using Godot;

public partial class BallOutline : Area2D
{
    public override void _Ready()
    {
        AreaEntered += (area) => GD.Print("entered");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("shoot"))
        {
            var ball = GetNode<Area2D>("../BallOutline2");
            ball.Transform = ball.Transform.Translated(Vector2.Left * 15);
            ball.ForceUpdateTransform();
            ForceUpdateTransform();
            GD.Print(OverlapsArea(ball));
            
        }
    }
}