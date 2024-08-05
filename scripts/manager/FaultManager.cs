using Godot;

public partial class FaultManager : Node
{
    
    [Signal] public delegate void FaultCommittedEventHandler(int faults);

    public int MaxFaults { get; private set; }
    
    public int Faults { get; private set; }
    
}