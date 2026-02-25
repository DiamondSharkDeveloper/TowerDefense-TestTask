using CodeBase.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Static Data/Monster")]
    public class MonsterStaticData : ScriptableObject
    {
        public CreatureTypeId id;
        public float speed;
        public int coinsPerKill;
        public float health;
        public float attackPower;
        public float attackDelay;
        public float stopDistance;
        public  float dieTime=6;
    }
}