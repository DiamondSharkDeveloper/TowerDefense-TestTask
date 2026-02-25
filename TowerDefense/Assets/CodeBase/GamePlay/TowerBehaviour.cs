using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.GamePlay.Enemys;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData.TowerDefense;
using UnityEngine;

namespace CodeBase.GamePlay
{
    public abstract class TowerBehaviour : MonoBehaviour
    {
        [SerializeField] protected Transform shootPoint;

        protected TowerStaticData Data;
        protected EnemyRegistry Registry;

        private float _nextAttackTime;

        protected readonly List<Enemy> TargetsBuffer = new List<Enemy>(64);

        private ProjectilePool _projectilePool;
        private GameObject _projectilePrefab;
        private IAssetProvider _assets;

        public void Init(TowerStaticData data, EnemyRegistry registry)
        {
            Data = data;
            Registry = registry;
        }

        public async Task InitVisuals(IAssetProvider assets)
        {
            _assets = assets;
            _projectilePool = new ProjectilePool();

            if (string.IsNullOrWhiteSpace(Data.projectileAddress))
                return;

            _projectilePrefab = await _assets.Load<GameObject>(Data.projectileAddress);
        }

        protected virtual void Update()
        {
            if (Data == null || Registry == null)
                return;

            if (Time.time < _nextAttackTime)
                return;

            _nextAttackTime = Time.time + Data.attackInterval;
            TickAttack();
        }

        protected Projectile SpawnProjectile()
        {
            if (_projectilePrefab == null)
                return null;

            Projectile p = _projectilePool.Take();
            if (p == null)
            {
                GameObject go = Instantiate(_projectilePrefab);
                p = go.GetComponent<Projectile>();
                if (p == null)
                    p = go.AddComponent<Projectile>();

                p.InitReturn(OnProjectileReturned);
            }

            Transform origin = shootPoint != null ? shootPoint : transform;
            p.transform.position = origin.position;
            p.gameObject.SetActive(true);
            return p;
        }

        private void OnProjectileReturned(Projectile projectile)
        {
            _projectilePool.Return(projectile);
        }

        protected abstract void TickAttack();

        protected Enemy FindNearestInRange(float range)
        {
            float rangeSqr = range * range;
            Transform self = transform;

            Enemy best = null;
            float bestSqr = float.MaxValue;

            IReadOnlyList<Enemy> active = Registry.Active;
            for (int i = 0; i < active.Count; i++)
            {
                Enemy e = active[i];
                if (e == null || !e.isActiveAndEnabled)
                    continue;

                Vector3 d = e.transform.position - self.position;
                float sqr = d.sqrMagnitude;
                if (sqr > rangeSqr)
                    continue;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = e;
                }
            }

            return best;
        }

        protected void CollectInRange(Vector3 center, float radius, List<Enemy> buffer, int maxCount)
        {
            buffer.Clear();

            float rSqr = radius * radius;

            IReadOnlyList<Enemy> active = Registry.Active;
            for (int i = 0; i < active.Count; i++)
            {
                if (buffer.Count >= maxCount)
                    return;

                Enemy e = active[i];
                if (e == null || !e.isActiveAndEnabled)
                    continue;

                Vector3 d = e.transform.position - center;
                if (d.sqrMagnitude <= rSqr)
                    buffer.Add(e);
            }
        }
    }
}