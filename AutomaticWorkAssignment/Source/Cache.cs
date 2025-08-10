using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Cache<T>
    {
        private Func<T> _getter;
        private T _cachedValue;

        private float _lifetime = 1;
        private bool _dirty;
        private float _lastGetTime;

        private bool IsDirty => _lastGetTime + _lifetime < Time.time || _dirty;

        public Cache(Func<T> getter = null, float lifetime = 1)
        {
            _getter = getter;
            _lifetime = lifetime;
        }

        public T Get()
        {
            if (_getter == null)
                throw new InvalidOperationException("Getter is not set.");

            if (IsDirty)
            {
                _cachedValue = _getter();
                _lastGetTime = Time.time;
                _dirty = false;
            }

            return _cachedValue;
        }

        public T Get(Func<T> getter)
        {
            _getter = getter;
            return Get();
        }

        public void MarkDirty() => _dirty = true;
    }
}
