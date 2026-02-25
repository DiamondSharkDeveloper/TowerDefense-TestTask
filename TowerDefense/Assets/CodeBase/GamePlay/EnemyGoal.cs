using System;
using UnityEngine;

namespace CodeBase.GamePlay
{
    public class EnemyGoal : MonoBehaviour
    {
        private Enemy _enemy;
        private Destructible _castle;
        private PooledObject _pooled;

        private Action<Enemy> _onReached;

        private float _damage;
        private float _reachDistanceSqr;

        private Transform _self;
        private Transform _castleTransform;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
            _self = transform;
        }

        public void Init(Enemy enemy, Destructible castle, float damage, float reachDistance, Action<Enemy> onReached)
        {
            _enemy = enemy;
            _castle = castle;
            _damage = damage;
            _onReached = onReached;

            _castleTransform = castle != null ? castle.transform : null;

            float d = Mathf.Max(0.05f, reachDistance);
            _reachDistanceSqr = d * d;
        }

        private void Update()
        {
            if (_enemy == null || _castleTransform == null)
                return;

            if (!_enemy.enabled)
                return;

            Vector3 delta = _castleTransform.position - _self.position;
            if (delta.sqrMagnitude > _reachDistanceSqr)
                return;

            _castle.Hit(_damage);
            _onReached?.Invoke(_enemy);

            if (_pooled != null)
            {
                _pooled.Release();
                return;
            }

            gameObject.SetActive(false);
        }
    }
}