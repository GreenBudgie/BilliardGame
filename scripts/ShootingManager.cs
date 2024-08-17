using Godot;

public partial class ShootingManager : Node2D
{
    
    private const float MinDragDistance = 8;
    private const float MaxDragDistance = 128;
    
    [Export]
    private Sprite2D _aimPosition;

    private AimState _aimState = AimState.None;
    
    private bool _isAiming;
    
    // Strength is a value between 0 and 1 that represents the strength of a shot
    private float _shotStrength;
    
    public override void _Process(double delta)
    {
        if (_aimState == AimState.Aiming)
        {
            HandleAiming();
        } else if (_aimState == AimState.Dragging)
        {
            HandleDragging();
        }
    }

    public void _TablePressed()
    {
        if (_aimState == AimState.None)
        {
            _aimPosition.Visible = true;
            _aimState = AimState.Aiming;
        } else if (_aimState == AimState.Aiming)
        {
            _aimState = AimState.Dragging;
        } else if (_aimState == AimState.Dragging)
        {
            InitializeShot();
            _aimState = AimState.None;
        }
    }
    
    public void _TableReleased()
    {
       
    }

    private void InitializeShot()
    {
        _aimPosition.Visible = false;
        
        if (Mathf.IsZeroApprox(_shotStrength))
        {
            return;
        }
        
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotInitialized, GetShotData());
    }

    private void HandleAiming()
    {
        var prevPosition = _aimPosition.GlobalPosition;
        var newPosition = GetGlobalMousePosition();

        if (prevPosition == newPosition)
        {
            return;
        }
        
        _aimPosition.GlobalPosition = newPosition;
        UpdateShotData();
    }

    private void HandleDragging()
    {
        var dragVector = GetGlobalMousePosition() - _aimPosition.GlobalPosition;
        var dragDistance = dragVector.Length();

        if (dragDistance < MinDragDistance)
        {
            if (_shotStrength != 0)
            {
                _shotStrength = 0;
                UpdateShotData();   
            }
            return;
        }
        
        var newStrength = Mathf.Clamp(dragDistance / MaxDragDistance, 0, 1);

        if (Mathf.IsEqualApprox(_shotStrength, newStrength))
        {
            return;
        }

        _shotStrength = newStrength;
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

    private enum AimState
    {
        None,
        Aiming,
        Dragging
    }
    
}