using CodeBase.Enums;
using UnityEngine;

namespace CodeBase.StaticData.TowerDefense
{
    [CreateAssetMenu(menuName = "Static Data/Tower Defense/Tower", fileName = "Tower_")]
    public class TowerStaticData : ScriptableObject
    {
        public TowerTypeId id;

        [Header("Targeting")]
        [Min(0.1f)] public float range = 6f;
        [Min(0.05f)] public float attackInterval = 0.5f;

        [Header("Damage")]
        [Min(0f)] public float damage = 10f;

        [Header("Projectile")]//sorry I`m out of time adding this logic without addressable and proper instantiation.
        public string projectileAddress;
        [Min(1f)] public float projectileSpeed = 15f;

        [Header("Tower2")]
        [Min(1)] public int targetsCount = 2;

        [Header("Tower1 AOE")]
        [Min(0f)] public float aoeRadius = 2f;
        [Min(1)] public int aoeMaxTargets = 3;
    }
}