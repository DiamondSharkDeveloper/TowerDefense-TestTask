using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.StaticData
{[Serializable]
    [CreateAssetMenu(fileName = "LevelData", menuName = "Static Data/Level")]
    public class LevelStaticData : ScriptableObject
    {
        public string levelKey;
        public List<EnemyWaveData> EnemyWaves=new List<EnemyWaveData>();
    }
    
}