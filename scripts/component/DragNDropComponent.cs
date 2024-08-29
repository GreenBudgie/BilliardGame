using Godot;

public partial class DragNDropComponent : Area2D
{
    private static readonly StringName DragAction = "drag";

    [Signal]
    public delegate void DragStartedEventHandler();

    [Signal]
    public delegate void DragUpdateEventHandler(Vector2 relativeGlobalPosition);

    [Signal]
    public delegate void DragEndedEventHandler();

    private Vector2? _initialClickPosition;

    public override void _Process(double delta)
    {
        if (!_initialClickPosition.HasValue)
        {
            return;
        }

        var relativeGlobalPosition = GetGlobalPosition() + GetGlobalMousePosition() - _initialClickPosition.Value;
        EmitSignal(SignalName.DragUpdate, relativeGlobalPosition);
    }

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is not InputEventMouseButton mbEvent)
        {
            return;
        }

        if (!_initialClickPosition.HasValue && mbEvent.IsActionPressed(DragAction))
        {
            _initialClickPosition = GetGlobalMousePosition();
            viewport.SetInputAsHandled();
            EmitSignal(SignalName.DragStarted);
        }

        if (_initialClickPosition.HasValue && mbEvent.IsActionReleased(DragAction))
        {
            _initialClickPosition = null;
            viewport.SetInputAsHandled();
            EmitSignal(SignalName.DragEnded);
        }
    }
}