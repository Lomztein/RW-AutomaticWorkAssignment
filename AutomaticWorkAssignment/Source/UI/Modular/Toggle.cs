using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Toggle<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly Func<T, bool> _valueGetter;
        private readonly Action<T, bool> _valueSetter;
        private readonly Func<T, string> _labelGetter;

        private Texture2D _toggleOnIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
        private Texture2D _toggleOffIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

        private float Size => AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public Toggle(Func<T, bool> valueGetter, Action<T, bool> valueSetter, Func<T, string> labelGetter)
        {
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
            _labelGetter = labelGetter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new(position, new Vector2(width, Size));
            (Rect label, Rect toggle) = Utils.SplitRectHorizontalRight(rect, Size);
            Widgets.Label(label, _labelGetter(pawnSetting));
            bool value = _valueGetter(pawnSetting);
            if (Widgets.ButtonImage(toggle, value ? _toggleOnIcon : _toggleOffIcon))
                value = !value;
            _valueSetter(pawnSetting, value);
            return Size;
        }
    }
}
