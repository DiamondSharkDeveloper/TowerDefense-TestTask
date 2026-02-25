using System.Collections.Generic;
using CodeBase.GamePlay.Enemys;

namespace CodeBase.GamePlay
{
    public class ProjectilePool
    {
        private readonly Queue<Projectile> _pool = new Queue<Projectile>();

        public Projectile Take()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            return null;
        }

        public void Return(Projectile projectile)
        {
            if (projectile == null)
                return;

            projectile.gameObject.SetActive(false);
            _pool.Enqueue(projectile);
        }
    }
}