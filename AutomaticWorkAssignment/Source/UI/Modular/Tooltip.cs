using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    internal class Tooltip<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private Func<T, string> _labelGetter;
        private IHandlerModule<T> _innerHandler;

        public Tooltip(Func<T, string> labelGetter, IHandlerModule<T> innerHandler)
        {
            _labelGetter = labelGetter;
            _innerHandler = innerHandler;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            float height = _innerHandler.Handle(position, width, pawnSetting);
            TooltipHandler.TipRegion(new Rect(position.x, position.y, width, height), _labelGetter(pawnSetting));
            return height;
        }
    }
}
