using CodeBase.GamePlay.Enemys;
using CodeBase.Services;

namespace CodeBase.Services.Enemies
{
    public interface IEnemyRegistryService : IService
    {
        EnemyRegistry Registry { get; }
    }
}