using Godot;

public static class Delayer
{
    
    public static SignalAwaiter Wait(this Node node, float time)
    {
        return node.ToSignal(node.GetTree().CreateTimer(time), Timer.SignalName.Timeout);
    }
    
}