using System;
using Godot;

public partial class ScoreCounterLabel : Label
{

    public override void _Ready()
    {
        EventBus.Instance.ScoreChanged += HandleScoreChange;
    }

    private void HandleScoreChange(int score)
    {
        Text = score.ToString();
    }

}