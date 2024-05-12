using Godot;

public partial class Cue : Node2D
{
    [Export] private CueBall _cueBall;

    [Export] private float _maxOffset = 130;

    [Node]
    private Sprite2D _sprite;

    private bool _isVisible;

    private Tween _alphaTween;

    public override void _Ready()
    {
        this.InitAttributes();
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the cue");
            QueueFree();
            return;
        }

        _sprite.Modulate = new Color(1, 1, 1, 0);

        _cueBall.ShotInitialized += HandleShotAnimation;
    }

    public override void _Process(double delta)
    {
        if (_cueBall.State == CueBall.BallState.ShotAnimation)
        {
            return;
        }
        
        if (_cueBall.State == CueBall.BallState.ShotPrepare)
        {
            HandleShotPreparation();
        }
        else
        {
            HideCue();
        }
    }

    private void HandleShotAnimation()
    {
        var shotTween = CreateTween();
        shotTween.TweenProperty(_sprite, "offset", new Vector2(0, _sprite.Offset.Y), 0.4)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        shotTween.Finished += () => _cueBall.PerformShot();
    }

    private void HandleShotPreparation()
    {
        Position = _cueBall.Position;
        LookAt(GetGlobalMousePosition());
        if (_cueBall.ShotData.Inverse)
        {
            Rotation += Mathf.Pi;
        }

        ShowCue();

        var offset = ShotStrengthUtil.GetCueOffsetForStrength(_cueBall.ShotData.Strength) + _cueBall.Radius;
        _sprite.Offset = new Vector2(-offset, _sprite.Offset.Y);
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