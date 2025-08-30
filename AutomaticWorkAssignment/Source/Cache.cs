using System;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Cache<T>
    {
        private T _cachedValue;

        private float _lifetime = 1;
        private float _lastSetTime;

        private bool IsDirty => _lastSetTime + _lifetime < Time.time;

        public Cache(float lifetime = 1)
        {
            _lifetime = lifetime;
        }

        public bool TryGet(out T value)
        {
            if (IsDirty)
            {
                value = default;
                return false;
            }
            value = _cachedValue;
            return true;
        }

        public T Set(T value)
        {
            _cachedValue = value;
            _lastSetTime = Time.time;
            return _cachedValue;
        }

        public void MarkDirty() => _lastSetTime = float.MinValue;
    }
}
