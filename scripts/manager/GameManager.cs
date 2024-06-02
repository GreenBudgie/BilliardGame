using Godot;

public partial class GameManager : Node
{

    private static Game _cachedGame;
    
    public static GameManager Instance { get; private set; }

    public static Game Game
    {
        get { return _cachedGame ??= Instance.GetNode<Game>("/root/Game"); }
    }

    public override void _Ready()
    {
        Instance = this;
    }

}