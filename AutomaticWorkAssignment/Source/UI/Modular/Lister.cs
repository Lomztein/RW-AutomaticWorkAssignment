using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Lister<T, E, O> : IHandlerModule<T> where T : IPawnSetting
    {
        private Action<Rect, E> _innerDrawer;
        private float _elementHeight;

        private readonly Func<Map, IEnumerable<O>> _optionGetter;
        private readonly Func<O, string> _optionLabelGetter;
        private readonly Func<O, Texture2D> _optionIconGetter;

        private readonly Func<T, IList<E>> _listGetter;
        private readonly Func<T, O, E> _onOptionSelected;

        private float ButtonSize => AutomaticWorkAssignmentSettings.UIButtonSizeBase;
        private float RemoveButtonSize = AutomaticWorkAssignmentSettings.UIInputSizeBase;

        public Lister(Action<Rect, E> innerDrawer, float elementHeight, Func<Map, IEnumerable<O>> optionGetter, Func<O, string> optionLabelGetter, Func<T, List<E>> listGetter, Func<T, O, E> onOptionSelected, Func<O, Texture2D> iconGetter = null)
        {
            _innerDrawer = innerDrawer;
            _elementHeight = elementHeight;

            _optionGetter = optionGetter;
            _optionLabelGetter = optionLabelGetter;
            _optionIconGetter = iconGetter;
         
            _listGetter = listGetter;
            _onOptionSelected = onOptionSelected;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            float y = 0;

            Rect rect = new(new Vector2(position.x + 4, position.y), new Vector2(width - 4, 0));
            RectAggregator aggregator = new(rect, GetHashCode(), margin: Vector2.zero);
            IList<E> list = _listGetter(pawnSetting);

            foreach (E element in list)
            {
                Rect elementRect = aggregator.NewRow(_elementHeight);
                _innerDrawer(elementRect, element);

                (Rect _, Rect right) = Utils.SplitRectHorizontalRight(elementRect, RemoveButtonSize);
                if (Widgets.ButtonText(right, "X"))
                {
                    Find.Root.StartCoroutine(DelayedRemove(list, element));
                }
            }

            Rect button = aggregator.NewRow(ButtonSize);
            if (Widgets.ButtonText(button, "AWA.Add".Translate()))
            {
                var options = GetFloatMenuOptions(pawnSetting).ToList();
                Find.WindowStack.Add(new FloatMenu(options));
            }

            y += aggregator.Rect.height;
            return y;
        }

        private IEnumerator DelayedRemove(IList<E> list, E element)
        {
            yield return new WaitForEndOfFrame();
            list.Remove(element);
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions(T pawnSetting)
        {
            var options = _optionGetter(Find.CurrentMap);
            foreach (var option in options)
            {
                yield return new FloatMenuOption(
                    _optionLabelGetter(option),
                    () => _listGetter(pawnSetting).Add(_onOptionSelected(pawnSetting, option)),
                    _optionIconGetter == null ? null : _optionIconGetter(option),
                    Color.white
                    );
            }
        }
    }
}
