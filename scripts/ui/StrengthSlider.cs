using Godot;

public partial class StrengthSlider : Control
{

    [Export]
    private VSlider _slider;

    public void _ShotStrengthChanged(float value)
    {
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotStrengthChanged, value);
    }
    
    public void _ShotStrengthSelected(bool valueChanged)
    {
        if (!valueChanged)
        {
            return;
        }
        
        EventBus.Instance.EmitSignal(EventBus.SignalName.ShotStrengthSelected, _slider.Value);

        _slider.Value = _slider.MinValue;
    }
    
}