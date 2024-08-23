using Godot;

public partial class BilliardManager : Node
{
    private static Billiard _cachedBilliard;

    public static BilliardManager Instance { get; private set; }

    public static Billiard Billiard
    {
        get { return _cachedBilliard ??= Instance.GetNode<Billiard>("/root/Billiard"); }
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("restart"))
        {
            _cachedBilliard = null;
            GetTree().ReloadCurrentScene();
        }

        if (Input.IsActionJustPressed("fullscreen"))
        {
            var mode = DisplayServer.WindowGetMode();
            if (mode == DisplayServer.WindowMode.Windowed)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            }
        }
    }
}