using Godot;

public partial class ShootingManager : Node2D
{
    
    [Export]
    private Marker2D _aimPosition;

    private bool _isAiming;
    private bool _hasAimPosition;
    private float _shotStrength;

    public override void _Ready()
    {
        EventBus.Instance.ShotStrengthChanged += _HandleShotStrengthChange;
        EventBus.Instance.ShotStrengthSelected += _InitializeShot;
    }

    public override void _Process(double delta)
    {
        HandleAiming();
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

    private void _HandleShotStrengthChange(float newShotStrength)
    {
        _shotStrength = newShotStrength;
        UpdateShotData();
    }

    private void _InitializeShot(float shotStrength)
    {
        if (shotStrength == 0)
        {
            return;
        }
        
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotInitialized, GetShotData());
    }

    private void HandleAiming()
    {
        if (!_isAiming)
        {
            return;
        }

        var prevPosition = _aimPosition.GlobalPosition;
        var newPosition = GetGlobalMousePosition();

        if (prevPosition == newPosition)
        {
            return;
        }
        
        _aimPosition.GlobalPosition = newPosition;
        UpdateShotData();
    }

    private void UpdateShotData()
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotDataChanged, GetShotData());
    }

    private ShotData GetShotData()
    {
        return new ShotData(_aimPosition.GlobalPosition, _shotStrength);
    }
    
}