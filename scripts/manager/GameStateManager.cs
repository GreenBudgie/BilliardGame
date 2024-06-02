using Godot;

public partial class GameStateManager : Node
{
    [Signal]
    public delegate void StateChangedEventHandler(GameState oldState, GameState newState);

    public GameState State { get; private set; } = GameState.ShotPreparation;

    public void ChangeState(GameState newState)
    {
        var oldState = State;
        State = newState;
        EmitSignal(SignalName.StateChanged, (int)oldState, (int)State);
    }
    
}