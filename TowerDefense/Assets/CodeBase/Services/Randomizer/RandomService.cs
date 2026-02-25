using Random = UnityEngine.Random;

namespace CodeBase.Services.Randomizer
{
    public class RandomService : IRandomService
    {
        public int Next(float min, float max) => (int)Random.Range(min, max);
    }
}