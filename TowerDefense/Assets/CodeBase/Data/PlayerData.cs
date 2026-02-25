using UnityEngine;

namespace CodeBase.StaticData
{
    public class PlayerData
    {
        public int BattleCoins;
        public int CannonDamage;
        public int CannonHealth;
        public float ExplosionRadius;
        public int ExplosionDamage;
        public string CurrentLevel;

        public PlayerData(int battleCoins, int cannonDamage, int cannonHealth, float explosionRadius, int explosionDamage, string currentLevel)
        {
            BattleCoins = battleCoins;
            CannonDamage = cannonDamage;
            CannonHealth = cannonHealth;
            ExplosionRadius = explosionRadius;
            ExplosionDamage = explosionDamage;
            CurrentLevel = currentLevel;
        }
    }
}