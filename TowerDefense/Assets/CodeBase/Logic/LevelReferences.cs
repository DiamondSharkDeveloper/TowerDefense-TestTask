using UnityEngine;

namespace CodeBase.Logic
{
    public class LevelReferences : MonoBehaviour
    {
        [Header("Enemy")]
        public Transform EnemySpawnPoint;

        [Header("Castle")]
        public Destructible CastleTarget;

        [Header("UI")]
        public Transform UiRoot;

        [Header("Towers")]
        public Transform Tower1SpawnPoint;
        public Transform Tower2SpawnPoint;
    }
}