using Godot;

public partial class Cue : Node2D
{
    [Export] private CueBall _cueBall;

    [Export] private float _maxOffset = 300;

    private Sprite2D _sprite;

    private bool _isVisible;

    private Tween _alphaTween;

    public override void _Ready()
    {
        if (_cueBall == null)
        {
            GD.PrintErr("Cue ball is not assigned for the cue");
            QueueFree();
            return;
        }

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _sprite.Modulate = new Color(1, 1, 1, 0);
    }

    public override void _Process(double delta)
    {
        Position = _cueBall.Position;
        LookAt(GetGlobalMousePosition());
        Rotation += Mathf.Pi;

        if (_cueBall.IsShooting)
        {
            ShowCue();
        }
        else
        {
            HideCue();
        }

        var shootVectorLength = _cueBall.ShootVector.Length();
        var weight = (shootVectorLength - _cueBall.MinShootStrength) /
                     (_cueBall.MaxShootStrength - _cueBall.MinShootStrength);
        var offset = Mathf.Lerp(0, _maxOffset, weight);
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
        _alphaTween.TweenProperty(_sprite, "modulate", new Color(1, 1, 1, 1), 0.5)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}