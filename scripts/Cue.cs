using Godot;

public partial class Cue : Node2D
{

    [Signal]
    public delegate void CueAnimationEndedEventHandler();
    
    [Export] private CueBall _cueBall;

    private Sprite2D _sprite;
    private Tween _alphaTween;
    private bool _isVisible;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _sprite.Modulate = new Color(1, 1, 1, 0);

        CueAnimationEnded += HideCue;
    }

    private void _HandleShotInitialization(ShotData shotData)
    {
        var shotTween = CreateTween();
        shotTween.TweenProperty(_sprite, "offset", new Vector2(0, _sprite.Offset.Y), 0.4)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        shotTween.Finished += () => EmitSignal(SignalName.CueAnimationEnded);
    }

    private void _HandleAimingStarted(Vector2 aimPosition)
    {
        if (_cueBall != null)
        {
            Position = _cueBall.Position;
        }
        
        UpdateCueOffset(0);
        ShowCue();
        _HandleAimPositionChange(aimPosition);
    }

    private void _HandleAimPositionChange(Vector2 aimPosition)
    {
        LookAt(aimPosition);
    }
    
    private void _HandleStrengthChange(float strength)
    {
        UpdateCueOffset(strength);
    }

    private void _HandleShotCancelled()
    {
        HideCue();
    }

    private void UpdateCueOffset(float strength)
    {
        var ballRadius = 8f; // Default fallback radius
        if (_cueBall != null)
        {
            ballRadius = _cueBall.Radius;
        }
        
        var offset = ShotStrengthUtil.GetCueOffsetForStrength(strength) + ballRadius;
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