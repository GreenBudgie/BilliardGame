using Godot;

public partial class EventBus : Node
{
    /*
     * Called whenever a ball is scored into the pocket. Also called when cue ball is scored.
     */
    [Signal]
    public delegate void BallScoredEventHandler(Ball ball, Pocket pocket);
    
    /*
     * Called whenever every sticker was applied and the score is ready to be processed by any other game mechanic.
     */
    [Signal]
    public delegate void ScoringEndedEventHandler(PocketScoreContext context);
    
    /*
    * Called when any ball has stopped rolling completely.
    */
    [Signal]
    public delegate void BallStoppedEventHandler(Ball ball);
    
    [Signal]
    public delegate void ScoreChangedEventHandler(int score);
    
    // Aiming and shooting
    
    [Signal]
    public delegate void ShotPerformedEventHandler(ShotData shotData);
    
    [Signal]
    public delegate void ShotInitializedEventHandler(ShotData shotData);
    
    [Signal]
    public delegate void ShotDataChangedEventHandler(ShotData shotData);
    
    [Signal]
    public delegate void ShotStrengthChangedEventHandler(float strength);
    
    [Signal]
    public delegate void ShotStrengthSelectedEventHandler(float strength);

    public static EventBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

}