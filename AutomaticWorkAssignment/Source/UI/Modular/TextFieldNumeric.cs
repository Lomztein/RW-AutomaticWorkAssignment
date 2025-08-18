using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class TextFieldNumeric<D, T> : IHandlerModule<T> where T : IPawnSetting where D : struct
    {
        private readonly Buffer<string> _buffer = new Buffer<string>();
        private D _value;

        private readonly Func<T, D> _getter;
        private readonly Action<T, D> _setter;

        private readonly float _min;
        private readonly float _max = float.MaxValue;

        private float InputSize => AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public TextFieldNumeric(Func<T, D> getter, Action<T, D> setter, float min = 0, float max = float.MaxValue)
        {
            _getter = getter;
            _setter = setter;
            _min = min;
            _max = max;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, InputSize));
            string buffer = _buffer.Get(pawnSetting);
            _value = _getter(pawnSetting);
            Widgets.TextFieldNumeric(rect, ref _value, ref buffer, _min, _max);
            _setter(pawnSetting, _value);
            _buffer.Set(pawnSetting, buffer);
            return rect.height;
        }
    }
}
