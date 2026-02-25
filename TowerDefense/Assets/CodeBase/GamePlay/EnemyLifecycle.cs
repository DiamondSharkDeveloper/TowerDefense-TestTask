using CodeBase.Infrastructure.Factory;
using UnityEngine;

namespace CodeBase.GamePlay.Enemys
{
    public class EnemyLifecycle : MonoBehaviour
    {
        private GameFactory _factory;
        private Enemy _enemy;
        private Destructible _destructible;

        private bool _subscribed;

        public void Init(GameFactory factory, Enemy enemy)
        {
            Unsubscribe();

            _factory = factory;
            _enemy = enemy;

            _destructible = enemy as Destructible;
            if (_destructible == null)
                _destructible = enemy.GetComponent<Destructible>();

            Subscribe();
        }

        private void OnDisable()
        {
            // Important for pooling: if object is returned to pool, we must not keep old subscriptions.
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed)
                return;

            if (_factory == null || _enemy == null || _destructible == null)
            {
                Debug.LogError("EnemyLifecycle: missing references (factory/enemy/destructible). Check enemy prefab setup.");
                return;
            }

            _destructible.OnDie -= OnDie;
            _destructible.OnDie += OnDie;

            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (_destructible != null)
                _destructible.OnDie -= OnDie;

            _subscribed = false;
        }

        private void OnDie(int coins)
        {
            if (_factory == null || _enemy == null)
                return;

            // This is the only place where "kill" is reported to factory.
            _factory.HandleEnemyDied(_enemy, coins);

            // Do not unsubscribe here. Death flow might be delayed (die animation) and object can be pooled later.
            // OnDisable will handle cleanup when pooled.
        }
    }
}