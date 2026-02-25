using System;
using CodeBase.Services;

namespace CodeBase.Services.Score
{
    public interface IScoreService : IService
    {
        int Score { get; }
        event Action<int> OnScoreChanged;

        void Reset();
        void Add(int amount);
    }
}