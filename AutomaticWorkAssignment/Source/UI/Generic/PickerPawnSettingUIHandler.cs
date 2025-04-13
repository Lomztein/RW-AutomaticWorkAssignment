using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.UI.Generic
{
    public class PickerPawnSettingUIHandler<T, E> : PawnSettingUIHandler<T> where T : IPawnSetting
    {
        private readonly Func<IEnumerable<E>> _optionGetter;
        private readonly Func<E, string> _optionLabelGetter;
        private readonly Func<T, string> _labelGetter;
        private readonly Action<T, E> _onSelected;

        private readonly float _pickButtonSize = 32;

        public PickerPawnSettingUIHandler(Func<IEnumerable<E>> optionGetter, Func<E, string> optionLabelGetter, Func<T, string> labelGetter, Action<T, E> onSelected)
        {
            _optionGetter = optionGetter;
            _optionLabelGetter = optionLabelGetter;
            _labelGetter = labelGetter;
            _onSelected = onSelected;
        }

        protected override float Handle(Vector2 position, float width, T pawnSetting)
        {
            float y = 0f;

            Rect buttonRect = new Rect(position, new Vector2(width, _pickButtonSize));
            buttonRect.y += y;

            if (Widgets.ButtonText(buttonRect, _labelGetter(pawnSetting)))
            {
                FloatMenuUtility.MakeMenu(_optionGetter(), _optionLabelGetter, x => () => _onSelected(pawnSetting, x));
            }

            y += buttonRect.height;
            return y;
        }
    }
}
