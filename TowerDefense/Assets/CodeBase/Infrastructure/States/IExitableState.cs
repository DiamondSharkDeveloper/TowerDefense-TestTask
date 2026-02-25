namespace CodeBase.Infrastructure.States
{
    public interface IExitableState
    {
        void Exit();
        bool IsOnPause();
    }
}