using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Data
{
    [Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        public Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
        [SerializeField]
        private List<TKey> _keys;
     
        [SerializeField]
        private List<TValue> _values;

        public void OnBeforeSerialize()
        {
            _keys = Dictionary.Keys.ToList();
            _values = Dictionary.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < _keys.Count; i++)
                Dictionary.Add(_keys[i], _values[i]);
        }
        
    }
}