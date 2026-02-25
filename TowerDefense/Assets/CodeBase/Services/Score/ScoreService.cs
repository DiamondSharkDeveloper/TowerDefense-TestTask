using System;

namespace CodeBase.Services.Score
{
    public class ScoreService : IScoreService
    {
        public int Score { get; private set; }

        public event Action<int> OnScoreChanged;

        public void Reset()
        {
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }
    }
}