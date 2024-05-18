using Godot;

public partial class CounterLabel : Label
{

    private int _count;
    
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            Text = _count.ToString();
        }
    }
    
    public override void _Ready()
    {
        Count = int.Parse(Text);
    }

}