using Godot;

public partial class AimManager : Node2D
{
    
    private Marker2D _aimPosition;

    private bool _isAiming;
    private bool _hasAimPosition;

    public override void _Ready()
    {
        _aimPosition = GetNode<Marker2D>("AimPosition");
    }

    public override void _Process(double delta)
    {
        if (!_isAiming)
        {
            return;
        }

        _aimPosition.GlobalPosition = GetGlobalMousePosition();
    }

    public Vector2? GetAimPositionIfPresent()
    {
        if (!_hasAimPosition)
        {
            return null;
        }
        
        return _aimPosition.GlobalPosition;
    }

    public void _StartAiming()
    {
        _isAiming = true;
        _hasAimPosition = true;
    }
    
    public void _StopAiming()
    {
        _isAiming = false;
    }

    public void _RemoveAimPosition()
    {
        _isAiming = false;
        _hasAimPosition = false;
    }
    
}