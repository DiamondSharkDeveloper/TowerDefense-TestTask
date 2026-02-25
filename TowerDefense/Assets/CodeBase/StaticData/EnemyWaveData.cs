using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace CodeBase.StaticData
{
    [Serializable] 
    public class EnemyWaveData
    {
        public float AppearanceTime;
        public List<CreatureOnWaveData> CreatureOnWaveData;
    }
}