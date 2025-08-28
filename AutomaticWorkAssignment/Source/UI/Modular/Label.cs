using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Label<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly Func<T, string> _labelGetter;
        private readonly TextAnchor _anchor;
        private readonly float _size;

        public Label(Func<T, string> labelGetter, TextAnchor textAnchor = TextAnchor.MiddleCenter, float size = AutomaticWorkAssignmentSettings.UIInputSizeBase)
        {
            _labelGetter = labelGetter;
            _size = size;
            _anchor = textAnchor;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _size));
            Text.Anchor = _anchor;
            Widgets.Label(Utils.ShrinkByMargin(rect, 2), _labelGetter(pawnSetting));
            Text.Anchor = TextAnchor.UpperLeft;
            return rect.height;
        }
    }
}
