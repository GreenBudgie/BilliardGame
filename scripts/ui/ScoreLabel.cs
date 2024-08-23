using System;
using Godot;

public partial class ScoreLabel : Label
{

    private static readonly string ScorePrefix = "Score: ";
    
    public override void _Ready()
    {
        ScoringManager.Instance.ScoreChanged += _HandleScoreChange;
    }

    private void _HandleScoreChange(int score)
    {
        Text = $"{ScorePrefix}{score}";
    }

}