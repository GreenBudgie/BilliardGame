using Godot;

public partial class GameStateManager : Node
{
    [Signal]
    public delegate void StateChangedEventHandler(GameState newState);

    public GameState State { get; private set; } = GameState.ShotPreparation;

    public static GameStateManager Instance { get; private set; }
    
    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public void ChangeState(GameState newState)
    {
        State = newState;
        EmitSignal(SignalName.StateChanged, (int)State);
    }
    
}