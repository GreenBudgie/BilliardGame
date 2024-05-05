using Billiard.scripts;
using Common;
using Godot;

public partial class ScoreManager : CanvasLayer
{
    public int Score { get; private set; }

    [Node] private Label _scoreLabel;

    public override void _Ready()
    {
        this.InitAttributes();

        EventBus.Instance.PocketScored += (ball, _) => _HandlePocketScore(ball);
    }

    public void _HandlePocketScore(PocketBall ball)
    {
        IncreaseScore(ball.Number);
    }

    private void IncreaseScore(int value)
    {
        Score += value;
        _scoreLabel.Text = Score.ToString();
    }
}