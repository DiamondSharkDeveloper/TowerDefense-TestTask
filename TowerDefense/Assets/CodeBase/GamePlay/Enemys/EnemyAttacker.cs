using CodeBase.GamePlay.Enemys;
using UnityEngine;

namespace Models.New_Enemy.Scripts
{
    public class EnemyAttacker : Enemy
    {
        [SerializeField] private Damager weapon;

        [SerializeField] private float hitDamage = 10.0f;
        [SerializeField] private float attackDistance = 5f;

        private bool _isInit;

        public Enemy Init(float speed, int coinsPerKill,
            float health,
            float attackPower,
            float attackDelay,
            float stopDistance,
            Destructible target,
            float dieTime = 6)
        {
            hitDamage = attackPower;

            EnemyInit(speed, stopDistance, target);
            DestructibleInit(health, dieTime, coinsPerKill);

            if (weapon != null)
                weapon.Init(hitDamage);

            if (!_isInit)
            {
                animationController.OnAttack += Attack;
                _isInit = true;
            }

            CancelInvoke(nameof(TryAttack));
            InvokeRepeating(nameof(TryAttack), attackDelay, attackDelay);

            return this;
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(TryAttack));
        }

        private bool CheckDistance()
        {
            if (Target == null)
                return false;

            Vector3 targetDirection = Target.transform.position - transform.position;
            return targetDirection.magnitude <= attackDistance;
        }

        private void TryAttack()
        {
            animationController.ChangeAttackAnimationStatus(CanAttack());
        }

        private void Attack()
        {
            if (weapon != null)
                weapon.Hit();
        }

        private bool CanAttack()
        {
            return IsAlive && CheckDistance() && !IsMove;
        }
    }
}