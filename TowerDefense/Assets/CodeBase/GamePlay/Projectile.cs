using System;
using CodeBase.GamePlay.Enemys;
using UnityEngine;

namespace CodeBase.GamePlay.Towers
{
    public class Projectile : MonoBehaviour
    {
        private Transform _self;
        private Enemy _target;

        private float _speed;
        private float _damage;

        private bool _useAoe;
        private float _aoeRadius;
        private int _aoeMaxTargets;

        private EnemyRegistry _registry;

        private Action<Projectile> _returnToPool;

        private void Awake()
        {
            _self = transform;

            Collider c = GetComponent<Collider>();
            if (c != null)
                c.enabled = false;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        public void InitReturn(Action<Projectile> returnToPool)
        {
            _returnToPool = returnToPool;
        }

        public void FireSingle(Enemy target, float speed, float damage)
        {
            _target = target;
            _speed = speed;
            _damage = damage;

            _useAoe = false;
            _registry = null;
        }

        public void FireAoe(Enemy target, float speed, float damage, EnemyRegistry registry, float aoeRadius, int aoeMaxTargets)
        {
            _target = target;
            _speed = speed;
            _damage = damage;

            _registry = registry;
            _useAoe = true;
            _aoeRadius = Mathf.Max(0f, aoeRadius);
            _aoeMaxTargets = Mathf.Max(1, aoeMaxTargets);
        }

        private void Update()
        {
            if (_target == null || !_target.isActiveAndEnabled)
            {
                Return();
                return;
            }

            Vector3 targetPos = _target.transform.position;
            Vector3 delta = targetPos - _self.position;

            float step = _speed * Time.deltaTime;
            if (delta.sqrMagnitude <= step * step)
            {
                _self.position = targetPos;
                OnHit(targetPos);
                return;
            }

            _self.position += delta.normalized * step;
        }

        private void OnHit(Vector3 hitPosition)
        {
            if (!_useAoe)
            {
                _target.Hit(_damage);
                Return();
                return;
            }

            if (_registry == null)
            {
                _target.Hit(_damage);
                Return();
                return;
            }

            float rSqr = _aoeRadius * _aoeRadius;

            int applied = 0;
            var active = _registry.Active;

            for (int i = 0; i < active.Count; i++)
            {
                if (applied >= _aoeMaxTargets)
                    break;

                Enemy e = active[i];
                if (e == null || !e.isActiveAndEnabled)
                    continue;

                Vector3 d = e.transform.position - hitPosition;
                if (d.sqrMagnitude <= rSqr)
                {
                    e.Hit(_damage);
                    applied++;
                }
            }

            Return();
        }

        private void Return()
        {
            _target = null;
            _registry = null;
            _returnToPool?.Invoke(this);
        }
    }
}