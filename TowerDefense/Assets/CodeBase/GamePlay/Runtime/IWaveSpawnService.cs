using CodeBase.Enums;

namespace CodeBase.GamePlay.Runtime
{
    public interface IWaveSpawnService
    {
        void Spawn(CreatureTypeId typeId);
    }
}