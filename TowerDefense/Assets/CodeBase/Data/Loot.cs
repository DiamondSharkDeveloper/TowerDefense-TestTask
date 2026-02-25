using System;
using UnityEngine.Serialization;

namespace CodeBase.Data
{
    [Serializable]
    public class Loot
    {
        public string name;
        public int value;

        public Loot(string name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }
}