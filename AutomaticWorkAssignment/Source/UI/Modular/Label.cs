using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Label<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private readonly Func<T, string> _labelGetter;

        private float Size => AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public Label(Func<T, string> labelGetter)
        {
            _labelGetter = labelGetter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, Size));
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, _labelGetter(pawnSetting));
            Text.Anchor = TextAnchor.UpperLeft;
            return rect.height;
        }
    }
}
