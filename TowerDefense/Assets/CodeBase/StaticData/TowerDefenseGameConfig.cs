using UnityEngine;

namespace CodeBase.StaticData.TowerDefense
{
    [CreateAssetMenu(menuName = "Static Data/Tower Defense/Game Config", fileName = "TowerDefenseGameConfig")]
    public class TowerDefenseGameConfig : ScriptableObject
    {
        [Header("Castle")]
        [Min(1)] public float castleMaxHp = 200f;
        [Min(0f)] public float castleDieTime = 0f;

        [Header("Gameplay")]
        [Min(0.05f)] public float enemyReachDistance = 0.6f;
    }
}