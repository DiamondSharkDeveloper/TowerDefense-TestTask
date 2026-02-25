using UnityEngine;

namespace CodeBase.GamePlay
{
    public class Tower1Behaviour : TowerBehaviour
    {
        protected override void TickAttack()
        {
            Enemy primary = FindNearestInRange(Data.range);
            if (primary == null)
                return;

            Projectile p = SpawnProjectile();
            if (p == null)
            {
                ApplyDirect(primary);
                return;
            }

            p.FireAoe(primary, Data.projectileSpeed, Data.damage, Registry, Data.aoeRadius, Data.aoeMaxTargets);
        }

        private void ApplyDirect(Enemy primary)
        {
            float radius = Mathf.Max(0f, Data.aoeRadius);
            if (radius <= 0.001f)
            {
                primary.Hit(Data.damage);
                return;
            }

            CollectInRange(primary.transform.position, radius, TargetsBuffer, Mathf.Max(1, Data.aoeMaxTargets));
            for (int i = 0; i < TargetsBuffer.Count; i++)
                TargetsBuffer[i].Hit(Data.damage);
        }
    }
}