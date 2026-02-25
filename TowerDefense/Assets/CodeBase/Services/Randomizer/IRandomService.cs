namespace CodeBase.Services.Randomizer
{
    public interface IRandomService : IService
    {
        int Next(float minValue, float maxValue);
    }
}