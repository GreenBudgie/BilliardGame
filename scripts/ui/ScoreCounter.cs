public partial class ScoreCounter : CounterLabel
{

    public override void _Ready()
    {
        this.InitAttributes();

        EventBus.Instance.PocketScored += (ball, _) => _HandlePocketScore(ball);
    }

    private void _HandlePocketScore(PocketBall ball)
    {
        Count += ball.Number;
    }

}