using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class TextField<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly Func<T, string> _getter;
        private readonly Action<T, string> _setter;

        private float InputSize => AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public TextField(Func<T, string> getter, Action<T, string> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, InputSize));
            string current = _getter(pawnSetting).ToString();
            current = Widgets.TextField(rect, current);
            _setter(pawnSetting, current);
            return rect.height;
        }
    }
}
