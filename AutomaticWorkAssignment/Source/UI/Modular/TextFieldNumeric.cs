using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class TextFieldNumeric<D, T> : IHandlerModule<T> where T : IPawnSetting where D : struct
    {
        private string _buffer;
        private D _value;

        private readonly Func<T, D> _getter;
        private readonly Action<T, D> _setter;

        private readonly float _min;
        private readonly float _max = float.MaxValue;

        private float InputSize => AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public TextFieldNumeric(string buffer, D value, Func<T, D> getter, Action<T, D> setter, float min = 0, float max = float.MaxValue)
        {
            _buffer = buffer;
            _value = value;
            _getter = getter;
            _setter = setter;
            _min = min;
            _max = max;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, InputSize));
            _buffer = _getter(pawnSetting).ToString();
            Widgets.TextFieldNumeric(rect, ref _value, ref _buffer, _min, _max);
            _value = (D)Convert.ChangeType(_value, typeof(D));
            _setter(pawnSetting, _value);
            return rect.height;
        }
    }
}
