using UnityEngine;

namespace CodeBase.GamePlay
{
    public class Tower2Behaviour : TowerBehaviour
    {
        protected override void TickAttack()
        {
            int count = Mathf.Max(1, Data.targetsCount);
            float range = Data.range;

            Enemy primary = FindNearestInRange(range);
            if (primary == null)
                return;

            TargetsBuffer.Clear();
            CollectInRange(transform.position, range, TargetsBuffer, count);

            if (TargetsBuffer.Count == 0)
                TargetsBuffer.Add(primary);

            for (int i = 0; i < TargetsBuffer.Count; i++)
            {
                Enemy target = TargetsBuffer[i];
                if (target == null || !target.isActiveAndEnabled)
                    continue;

                Projectile p = SpawnProjectile();
                if (p == null)
                {
                    target.Hit(Data.damage);
                    continue;
                }

                p.FireSingle(target, Data.projectileSpeed, Data.damage);
            }
        }
    }
}