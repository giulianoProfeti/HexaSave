namespace DemoGame.Gameplay
{
    /// <summary>O resultado de uma partida, entregue pelo RunController quando ela termina.</summary>
    public readonly struct RunResult
    {
        public RunResult(int score, int coinsCollected, float durationSeconds)
        {
            Score = score;
            CoinsCollected = coinsCollected;
            DurationSeconds = durationSeconds;
        }

        public int Score { get; }
        public int CoinsCollected { get; }
        public float DurationSeconds { get; }
    }
}