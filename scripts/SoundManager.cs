using Godot;

public partial class SoundManager : Node
{
    public static SoundManager Instance;

    private PackedScene _positionalSoundScene;

    public override void _Ready()
    {
        Instance = this;

        _positionalSoundScene = GD.Load<PackedScene>("res://scenes/positional_sound.tscn");
    }

    public PositionalSound PlayPositionalSound(Node2D node, AudioStream sound)
    {
        var soundNode = _positionalSoundScene.Instantiate<PositionalSound>();
        node.AddChild(soundNode);
        soundNode.Stream = sound;
        soundNode.Play();
        return soundNode;
    }
}