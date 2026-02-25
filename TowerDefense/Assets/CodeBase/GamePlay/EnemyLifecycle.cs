using UnityEngine;

namespace CodeBase.GamePlay
{
    public class EnemyLifecycle : MonoBehaviour
    {
        private GameFactoryProxy _proxy;
        private Enemy _enemy;

        public void Init(CodeBase.Infrastructure.Factory.GameFactory factory, Enemy enemy)
        {
            _enemy = enemy;

            _proxy = GetComponent<GameFactoryProxy>();
            if (_proxy == null)
                _proxy = gameObject.AddComponent<GameFactoryProxy>();

            _proxy.SetFactory(factory);
        }

        private void OnEnable()
        {
            if (_enemy != null)
                _enemy.OnDie += OnDie;
        }

        private void OnDisable()
        {
            if (_enemy != null)
                _enemy.OnDie -= OnDie;
        }

        private void OnDie(int coins)
        {
            _proxy.NotifyEnemyDied(_enemy, coins);
        }
    }

    public class GameFactoryProxy : MonoBehaviour
    {
        private CodeBase.Infrastructure.Factory.GameFactory _factory;

        public void SetFactory(CodeBase.Infrastructure.Factory.GameFactory factory)
        {
            _factory = factory;
        }

        public void NotifyEnemyDied(Enemy enemy, int coins)
        {
            if (_factory != null)
                _factory.HandleEnemyDied(enemy, coins);
        }
    }
}