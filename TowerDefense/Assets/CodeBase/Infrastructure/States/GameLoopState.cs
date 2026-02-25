namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        public GameLoopState(GameStateMachine stateMachine)
        {
        }

        public void Exit()
        {
        }

        public bool IsOnPause()
        {
            return false;
        }

        public void Enter()
        {
        }
    }
}