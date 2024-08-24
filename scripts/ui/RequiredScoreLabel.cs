using System;
using Godot;

public partial class RequiredScoreLabel : Label
{
    private static readonly string ScorePrefix = "Required: ";

    public override void _Ready()
    {
        var requiredScore = ScoringManager.Instance.RequiredScore;
        Text = $"{ScorePrefix}{requiredScore}";
    }
}