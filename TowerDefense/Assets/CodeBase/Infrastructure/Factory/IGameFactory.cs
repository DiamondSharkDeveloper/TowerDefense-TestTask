using System;
using System.Threading.Tasks;
using CodeBase.Enums;
using CodeBase.GamePlay;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        void SetLevelReferences(LevelReferences references);

        void CreateEnemyWaves(LevelStaticData levelStaticData, Action onWin, Action onLose);

        Task<Enemy> CreateCreature(CreatureTypeId typeId);

        Task WarmUp();

        void Cleanup();

        void ShowEndGameOverlay(bool isWin);
    }
}