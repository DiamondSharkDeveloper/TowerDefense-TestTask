using JetBrains.Annotations;

namespace CodeBase.Infrastructure.States
{
    public interface IState:IExitableState
    {
        void Enter(); 
    }
    public interface IPayloadedState<TPayload> : IExitableState
    {
        void Enter([CanBeNull] TPayload isGameRun);
    }
}