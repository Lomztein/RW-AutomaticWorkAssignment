using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment
{
    public class CacheDict<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private Dictionary<TKey, float> _lastGetTime = new Dictionary<TKey, float>();

        private float _lifetime = 1;
        
        private bool IsDirty(TKey key)
        {
            if (_lastGetTime.TryGetValue(key, out float value))
                return value + _lifetime < Time.time;
            return true;
        }

        public CacheDict(float lifetime = 1)
        {
            _lifetime = lifetime;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (IsDirty(key))
            {
                SetLastGetTime(key, Time.time);
                value = default;
                return false;
            }

            value = GetCache(key);
            return true;
        }

        public void Set(TKey key, TValue value)
            => Cache(key, value);

        private void Cache(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                _dict.Add(key, value);
            else
                _dict[key] = value;
        }

        private TValue GetCache(TKey key)
        {
            if (_dict.TryGetValue(key, out TValue value))
                return value;
            return default;
        }

        private void SetLastGetTime (TKey key, float value)
        {
            if (!_lastGetTime.ContainsKey(key))
                _lastGetTime.Add(key, value);
            else
                _lastGetTime[key] = value;
        }

        public void MarkDirty(TKey key) 
            => SetLastGetTime(key, float.MinValue);
    }
}
