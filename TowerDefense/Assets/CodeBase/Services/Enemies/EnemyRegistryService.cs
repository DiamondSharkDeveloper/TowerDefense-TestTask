using CodeBase.GamePlay.Enemys;

namespace CodeBase.Services.Enemies
{
    public class EnemyRegistryService : IEnemyRegistryService
    {
        public EnemyRegistry Registry { get; } = new EnemyRegistry();
    }
}