namespace CodeBase.GamePlay
{
    public interface IEnemyAnimationController
    {
        void PlayWinAnimation();
        void PlayDieAnimation();
        void PlayThrowAnimation();
        void PlayHitAnimation();
        void ChangeWalkAnimationStatus(bool isWalking);
        void ChangeAttackAnimationStatus(bool isAttacking);
    }
}