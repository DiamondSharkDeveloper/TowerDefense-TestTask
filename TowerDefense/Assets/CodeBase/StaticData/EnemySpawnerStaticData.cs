using System;
using CodeBase.Enums;
using UnityEngine;

namespace CodeBase.StaticData
{
    [Serializable]
    public class EnemySpawnerStaticData
    {
        public string Id;
        public CreatureTypeId creatureTypeId;
        public Vector3 Position;

        public EnemySpawnerStaticData(string id, CreatureTypeId creatureTypeId, Vector3 position)
        {
            Id = id;
            this.creatureTypeId = creatureTypeId;
            Position = position;
        }
    }
}