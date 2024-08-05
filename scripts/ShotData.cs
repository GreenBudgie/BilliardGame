using Godot;

public partial class ShotData : RefCounted
{
    
    public Vector2 AimPosition { get; private set; }
    public float Strength { get; private set; }

    public ShotData(Vector2 aimPosition, float strength)
    {
        AimPosition = aimPosition;
        Strength = strength;
    }

    public ShotData()
    {
    }
}