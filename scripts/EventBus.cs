using Godot;

public partial class EventBus : Node
{
    /*
     * Called whenever a ball is scored into the pocket
     */
    [Signal]
    public delegate void PocketScoredEventHandler(PocketBall pocketBall, Pocket pocket);

    /*
     * Called whenever a cue ball is scored into the pocket
     */
    [Signal]
    public delegate void CueBallScoredEventHandler(CueBall cueBall, Pocket pocket);

    public static EventBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

}