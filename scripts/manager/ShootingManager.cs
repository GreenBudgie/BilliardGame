using Godot;

public partial class ShootingManager : Node2D
{
    
    /// <summary>
    /// Fired when the player clicked on the table and started aiming.
    /// </summary>
    [Signal]
    public delegate void AimingStartedEventHandler(Vector2 aimPosition);
    
    /// <summary>
    /// Fired when aim position is changed
    /// </summary>
    [Signal]
    public delegate void AimPositionChangedEventHandler(Vector2 aimPosition);
    
    /// <summary>
    /// Fired when aim position is changed
    /// </summary>
    [Signal]
    public delegate void StrengthChangedEventHandler(float strength);

    /// <summary>
    /// Fired when aiming or dragging has been cancelled
    /// </summary>
    [Signal]
    public delegate void ShotCancelledEventHandler();
    
    /// <summary>
    /// Fired when the shot is initialized and cue animation should start playing
    /// </summary>
    [Signal]
    public delegate void ShotInitializedEventHandler(ShotData shotData);
    
    /// <summary>
    /// Fired when the shot is performed and the cue ball should start moving
    /// </summary>
    [Signal]
    public delegate void ShotPerformedEventHandler(ShotData shotData);

    private const float MaxDragDistance = 128;

    [Export] private Sprite2D _aimPosition;
    [Export] private ReferenceRect _tableAreaRect;

    private AimState _aimState = AimState.Preparing;
    private float _shotStrength;

    public override void _Ready()
    {
        GameStateManager.Instance.StateChanged += _HandleGameStateChange;
    }

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

    public void _PerformShot()
    {
        EmitSignal(SignalName.ShotPerformed, GetShotData());
    }

    private void _HandleGameStateChange(GameState state)
    {
        if (state == GameState.ShotPreparation)
        {
            _aimState = AimState.Preparing;
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

        EmitSignal(SignalName.ShotInitialized, GetShotData());
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
        EmitSignal(SignalName.AimPositionChanged, newPosition);
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
        EmitSignal(SignalName.StrengthChanged, newStrength);
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
        EmitSignal(SignalName.AimingStarted, _aimPosition.GlobalPosition);
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
        EmitSignal(SignalName.ShotCancelled);
    }

    private enum AimState
    {
        Preparing,
        Aiming,
        Dragging,
        Waiting
    }
}