using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    internal class Clickable<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private Action<T> _onClick;
        private Func<T, string> _labelGetter;

        private readonly float _buttonSize = 32;

        public Clickable(Action<T> onClick, Func<T, string> labelGetter)
        {
            _onClick = onClick;
            _labelGetter = labelGetter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, _labelGetter(pawnSetting)))
            {
                _onClick(pawnSetting);
            }
            return _buttonSize;
        }
    }
}
