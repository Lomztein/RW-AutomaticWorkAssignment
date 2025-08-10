using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.UI.Generic
{
    public class PickerPawnSettingUIHandler<T, E> : PawnSettingUIHandler<T> where T : IPawnSetting
    {
        private readonly Func<Map, IEnumerable<E>> _optionGetter;
        private readonly Func<E, string> _optionLabelGetter;
        private readonly Func<T, string> _labelGetter;
        private readonly Action<T, E> _onSelected;
        private readonly Func<E, Texture2D> _iconGetter;

        private readonly float _pickButtonSize = 32;

        public PickerPawnSettingUIHandler(Func<Map, IEnumerable<E>> optionGetter, Func<E, string> optionLabelGetter, Func<T, string> labelGetter, Action<T, E> onSelected, Func<E, Texture2D> iconGetter = null)
        {
            _optionGetter = optionGetter;
            _optionLabelGetter = optionLabelGetter;
            _labelGetter = labelGetter;
            _onSelected = onSelected;
            _iconGetter = iconGetter;
        }

        protected override float Handle(Vector2 position, float width, T pawnSetting)
        {
            float y = 0f;

            Rect buttonRect = new Rect(position, new Vector2(width, _pickButtonSize));
            buttonRect.y += y;

            if (Widgets.ButtonText(buttonRect, _labelGetter(pawnSetting)))
            {
                var options = GetFloatMenuOptions(pawnSetting).ToList();
                Find.WindowStack.Add(new FloatMenu(options));
            }

            y += buttonRect.height;
            return y;
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions(T pawnSetting)
        {
            var options = _optionGetter(Find.CurrentMap);
            foreach (var option in options)
            {
                yield return new FloatMenuOption(
                    _optionLabelGetter(option),
                    () => _onSelected(pawnSetting, option),
                    _iconGetter == null ? null : _iconGetter(option),
                    Color.white
                    );
            }
        }
    }
}
