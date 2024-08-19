using System.Collections.Generic;
using Godot;

public partial class TrajectoryDrawer : Node2D
{
    
    private const float MaxRayCastDistance = 1500;
    
    [Export] private CueBall _cueBall;
    
    private Line2D _trajectory;
    private RayCast2D _rayCast;

    public override void _Ready()
    {
        _trajectory = GetNode<Line2D>("Trajectory");
        _rayCast = GetNode<RayCast2D>("RayCast");
    }

    private void _HandleAimPositionChange(Vector2 aimPosition)
    {
        if (_cueBall != null)
        {
            Position = _cueBall.Position;
        }
        
        _trajectory.ClearPoints();

        var relativeAimPosition = aimPosition - GlobalPosition;
        var rayCastPosition = relativeAimPosition.Normalized() * MaxRayCastDistance;
        _rayCast.TargetPosition = rayCastPosition;
        _rayCast.ForceRaycastUpdate();

        if (!_rayCast.IsColliding())
        {
            return;
        }

        var collisionPoint = _rayCast.GetCollisionPoint();
        _trajectory.AddPoint(Vector2.Zero);
        _trajectory.AddPoint(collisionPoint - _trajectory.GlobalPosition);
    }
    
    private void _HandleShotInitialization(ShotData newShotData)
    {
        _trajectory.ClearPoints();
    }
    
    private void _HandleAimingStarted(Vector2 aimPosition)
    {
        _HandleAimPositionChange(aimPosition);
    }
    
    private void _HandleShotCancelled()
    {
        _trajectory.ClearPoints();
    }
    
}