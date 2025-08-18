using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Buffer<T>
    {
        // Dict will never be cleared throughout the game session, though the memory impact is so small that this (hopefully) isn't an issue.
        private readonly Dictionary<object, T> _dict = new Dictionary<object, T>();

        public T Get(object key)
        {
            if (_dict.ContainsKey(key))
                return _dict[key];
            _dict.Add(key, default);
            return default;
        }

        public void Set(object key, T value)
            => _dict[key] = value;
    }
}
