using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Splitter<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly IHandlerModule<T>[] _parts;

        public Splitter(params IHandlerModule<T>[] parts)
        {
            _parts = parts;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            float partWidth = width / _parts.Length;
            float height = 0;
            for (int i = 0;  i < _parts.Length; i++) 
            {
                float x = position.x + partWidth * i;
                float partHeight = _parts[i].Handle(new Vector2(x, position.y), partWidth, pawnSetting);
                height = Mathf.Max(height, partHeight);
            }
            return height;
        }
    }
}
