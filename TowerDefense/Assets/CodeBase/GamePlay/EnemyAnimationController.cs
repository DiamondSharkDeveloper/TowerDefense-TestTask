using System;
using UnityEngine;

namespace CodeBase.GamePlay
{
    public class EnemyAnimationController : MonoBehaviour, IEnemyAnimationController
    {
        [SerializeField] private Animator animator;
        private static readonly int Win = Animator.StringToHash("Win");
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Throw = Animator.StringToHash("Throw");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int IsWalk = Animator.StringToHash("IsWalk");
        private static readonly int IsAttack = Animator.StringToHash("IsAttack");
        public Action OnThrow;
        public Action OnAttack;

        public void PlayWinAnimation()
        {
            animator.SetTrigger(Win);
        }

        public void PlayDieAnimation()
        {
            animator.SetTrigger(Die);
        }

        public void PlayThrowAnimation()
        {
            animator.SetTrigger(Throw);
        }

        public void PlayHitAnimation()
        {
            animator.SetTrigger(Hit);
        }

        public void ChangeWalkAnimationStatus(bool isWalking)
        {
            animator.SetBool(IsWalk, isWalking);
        }

        public void ChangeAttackAnimationStatus(bool isAttacking)
        {
            animator.SetBool(IsAttack, isAttacking);
        }

        public void OnThrowAnimation()
        {
            OnThrow?.Invoke();
        }
        public void OnAttackAnimation()
        {
            OnAttack?.Invoke();
        }
    }
}