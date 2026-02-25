using Unity.VisualScripting;
using UnityEngine;

namespace CodeBase.GamePlay.Enemys
{
    public class TreeEnemy : Enemy
    {
        [SerializeField] private float hitDamage = 10.0f;

        public Enemy Init(float speed, int coinsPerKill,
            float health,
            float attackPower,
            float attackDelay,
            float stopDistance,Destructible target,
            float dieTime = 6)
        {
            hitDamage = attackPower;
            EnemyInit(speed,stopDistance,target);
            DestructibleInit(health,dieTime,coinsPerKill);
            return this;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Destructible target = collision.gameObject.GetComponent<Destructible>();
            if (target)
            {
                if (GameObject.Equals(collision.gameObject, target.gameObject))
                {
                    target.Hit(hitDamage);
                    StartCoroutine(Die(0));
                }
            }
        }
    }
}