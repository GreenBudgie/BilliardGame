using Godot;

public partial class EffectLabel : Label
{
    public override void _EnterTree()
    {
        var finalPosition = new Vector2(GD.RandRange(-15, 15), GD.RandRange(-20, -30));
        var tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(this, "position", finalPosition, 0.5)
            .AsRelative()
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.5);
        tween.Finished += QueueFree;
    }

    public static EffectLabel Create()
    {
        return GD.Load<PackedScene>("res://scenes/effects/effect_label.tscn").Instantiate<EffectLabel>();
    }
    
}