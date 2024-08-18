using Godot;

public partial class Cue : Node2D
{
    [Export] private CueBall _cueBall;

    private Sprite2D _sprite;

    private bool _isVisible;

    private Tween _alphaTween;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the cue");
            QueueFree();
            return;
        }

        _sprite.Modulate = new Color(1, 1, 1, 0);

        EventBus.Instance.ShotInitialized += _HandleShotAnimation;
        EventBus.Instance.ShotDataChanged += _HandleShotDataChange;
        EventBus.Instance.ShotPerformed += _HandleShotPerformed;
        EventBus.Instance.AimingStarted += _HandleAimingStarted;
        EventBus.Instance.ShotCancelled += _HandleAimingCancelled;
    }

    private void _HandleShotAnimation(ShotData shotData)
    {
        var shotTween = CreateTween();
        shotTween.TweenProperty(_sprite, "offset", new Vector2(0, _sprite.Offset.Y), 0.4)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        shotTween.Finished += () => EventBus.Instance.EmitSignal(EventBus.SignalName.ShotPerformed, shotData);
    }

    private void _HandleAimingStarted(ShotData initialShotData)
    {
        ShowCue();
        _HandleShotDataChange(initialShotData);
    }

    private void _HandleShotDataChange(ShotData shotData)
    {
        Position = _cueBall.Position;
        LookAt(shotData.AimPosition);

        var offset = ShotStrengthUtil.GetCueOffsetForStrength(shotData.Strength) + _cueBall.Radius;
        _sprite.Offset = new Vector2(-offset, _sprite.Offset.Y);
    }

    private void _HandleShotPerformed(ShotData shotData)
    {
        HideCue();
    }

    private void _HandleAimingCancelled()
    {
        HideCue();
    }

    private void HideCue()
    {
        if (!_isVisible)
        {
            return;
        }
        
        _isVisible = false;

        _alphaTween?.Kill();
        _alphaTween = CreateTween();
        _alphaTween.TweenProperty(_sprite, "modulate", new Color(1, 1, 1, 0), 0.5)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void ShowCue()
    {
        if (_isVisible)
        {
            return;
        }
        
        _isVisible = true;

        _alphaTween?.Kill();
        _alphaTween = CreateTween();
        _alphaTween.TweenProperty(_sprite, "modulate", new Color(1, 1, 1), 0.5)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}