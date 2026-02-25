using System;
using CodeBase.Services;
using Unity.VisualScripting;

namespace CodeBase.Infrastructure.States
{
    public interface IGameStateMachine : IService
    {
        void Enter<TState>() where TState : class, IState;
        void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>;
        TState GetState<TState>() where TState : class, IExitableState;
        void ChangeStateToPrevious();
        TState GetCurrentState<TState>() where TState : class, IExitableState;
        public event Action<IExitableState> OnStateChange;
    }
}