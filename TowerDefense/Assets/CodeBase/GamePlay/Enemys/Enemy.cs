using CodeBase.GamePlay.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.GamePlay.Enemys
{
    public class Enemy : Destructible
    {
        [SerializeField] protected NavMeshAgent navMeshAgent;
        [SerializeField] protected EnemyAnimationController animationController;

        protected Destructible Target;
        protected bool IsMove;

        public void EnemyInit(float speed, float stopDistance, Destructible target)
        {
            navMeshAgent.speed = speed;
            navMeshAgent.stoppingDistance = stopDistance;
            Target = target;
        }

        private void OnEnable()
        {
            OnHit += TakeHit;
            OnDie += Die;

            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.ResetPath();
            }
        }

        private void OnDisable()
        {
            OnHit -= TakeHit;
            OnDie -= Die;

            if (navMeshAgent != null)
            {
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
            }

            IsMove = false;
        }

        private void Move()
        {
            if (!IsAlive || Target == null)
                return;

            navMeshAgent.transform.LookAt(Target.transform.position);
            navMeshAgent.SetDestination(Target.transform.position);

            IsMove = navMeshAgent.velocity.magnitude != 0;
            animationController.ChangeWalkAnimationStatus(IsMove);
        }

        private void TakeHit()
        {
            if (navMeshAgent != null)
                navMeshAgent.velocity = Vector3.zero;

            animationController.PlayHitAnimation();
        }

        private void Die(int coinPerKill)
        {
            if (navMeshAgent != null)
                navMeshAgent.isStopped = true;

            animationController.PlayDieAnimation();
        }

        private void Update()
        {
            if (IsAlive)
                Move();
        }
    }
}