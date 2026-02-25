using System;
using System.Threading.Tasks;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        void SetLevelReferences(LevelReferences references);
        void CreateEnemyWaves(LevelStaticData staticData, Action onWin, Action onLose);

        void Cleanup();
        Task WarmUp();
    }
}