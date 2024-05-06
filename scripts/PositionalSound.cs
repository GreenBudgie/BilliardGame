using Godot;
using System;

public partial class PositionalSound : AudioStreamPlayer2D
{
    public override void _Ready()
    {
        Finished += QueueFree;
    }

    public PositionalSound RandomizePitch(float minPitch, float maxPitch)
    {
        PitchScale = (float)GD.RandRange(minPitch, maxPitch);
        return this;
    }
    
    public PositionalSound RandomPitchOffset(float minMaxAmount)
    {
        PitchScale = (float)GD.RandRange(PitchScale - minMaxAmount, PitchScale + minMaxAmount);
        return this;
    }
    
    public PositionalSound RandomVolumeOffset(float minMaxAmount)
    {
        VolumeDb = (float)GD.RandRange(VolumeDb - minMaxAmount, VolumeDb + minMaxAmount);
        return this;
    }
}