public struct ScoreChangedEvent
{
    public int Score { get; }
    public int HighScore { get; }

    public ScoreChangedEvent(int score, int highScore)
    {
        Score = score;
        HighScore = highScore;
    }
}