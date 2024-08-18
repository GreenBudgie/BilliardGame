using Godot;

public partial class ShootingManager : Node2D
{

    private const float MaxDragDistance = 128;
    
    [Export] private Sprite2D _aimPosition;

    [Export] private ReferenceRect _tableAreaRect;

    private AimState _aimState = AimState.Preparing;

    // Strength is a value between 0 and 1 that represents the strength of a shot
    private float _shotStrength;

    public override void _Process(double delta)
    {
        if (_aimState == AimState.Aiming)
        {
            HandleAiming();
        }
        else if (_aimState == AimState.Dragging)
        {
            HandleDragging();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mbEvent)
        {
            return;
        }

        if (_aimState == AimState.Waiting)
        {
            return;
        }

        var isShotPressed = mbEvent.IsActionPressed("shot");
        var isShotCancelled = mbEvent.IsActionPressed("cancel_shot");
        
        if (_aimState == AimState.Preparing)
        {
            if (!isShotPressed || !IsMouseInsideTableArea())
            {
                return;
            }
            
            GetViewport().SetInputAsHandled();
            StartAiming();
            return;
        }
        
        if (isShotCancelled)
        {
            GetViewport().SetInputAsHandled();
            CancelShot();
            return;
        }
        
        if (_aimState == AimState.Aiming)
        {
            if (!isShotPressed)
            {
                return;
            }
            
            GetViewport().SetInputAsHandled();
            StartDragging();
            return;
        }

        if (_aimState == AimState.Dragging && isShotPressed)
        {
            GetViewport().SetInputAsHandled();
            InitializeShot();
        }
    }

    private void InitializeShot()
    {
        if (Mathf.IsZeroApprox(_shotStrength))
        {
            CancelShot();
            return;
        }

        _aimState = AimState.Waiting;
        _aimPosition.Visible = false;
        
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

    private bool IsMouseInsideTableArea()
    {
        return _tableAreaRect.GetGlobalRect().HasPoint(GetGlobalMousePosition());
    }

    private void StartAiming()
    {
        _aimPosition.GlobalPosition = GetGlobalMousePosition();
        _aimPosition.Visible = true;
        _aimState = AimState.Aiming;
        EventBus.Instance.EmitSignal(EventBus.SignalName.AimingStarted, GetShotData());
    }
    
    private void StartDragging()
    {
        _aimState = AimState.Dragging;
    }

    private void CancelShot()
    {
        _shotStrength = 0;
        _aimPosition.Visible = false;
        _aimState = AimState.Preparing;
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotCancelled);
    }
    
    private enum AimState
    {
        Preparing,
        Aiming,
        Dragging,
        Waiting
    }
}