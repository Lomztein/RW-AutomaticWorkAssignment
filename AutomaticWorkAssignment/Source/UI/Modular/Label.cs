using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Label<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly Func<T, string> _labelGetter;
        private float _size;

        public Label(Func<T, string> labelGetter, float size = AutomaticWorkAssignmentSettings.UIInputSizeBase)
        {
            _labelGetter = labelGetter;
            _size = size;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _size));
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, _labelGetter(pawnSetting));
            Text.Anchor = TextAnchor.UpperLeft;
            return rect.height;
        }
    }
}
