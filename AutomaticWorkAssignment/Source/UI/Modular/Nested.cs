using Lomzie.AutomaticWorkAssignment.Defs;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Nested<T, D> : IHandlerModule<T> where T : IPawnSetting where D : PawnSettingDef
    {
        private float AddButtonSize => AutomaticWorkAssignmentSettings.UIButtonSizeBase;

        private readonly Func<T, IPawnSetting> _getter;
        private readonly Action<T, IPawnSetting> _setter;

        public Nested(Func<T, IPawnSetting> getter, Action<T, IPawnSetting> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            const int inset = 8;
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: inset), GetHashCode(), new(0, 1));
            IPawnSetting current = _getter(pawnSetting);

            if (current != null)
            {
                WorkManagerWindow.DoPawnSetting(
                    ref layout,
                    current,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)));
            }
            else
            {
                Rect addSettingButtonRect = layout.NewRow(AddButtonSize);
                if (Widgets.ButtonText(addSettingButtonRect, "AWA.NestedSettingSelect".Translate()))
                {
                    FloatMenuUtility.MakeMenu(GetDefs(), x => x.LabelCap, x => () => _setter(pawnSetting, PawnSetting.CreateFrom<IPawnSetting>(x)));
                }
            }

            return layout.Rect.height;
        }

        private IEnumerable<D> GetDefs()
            => DefDatabase<D>.AllDefs;

        private IEnumerator DelayedRemoveInnerSetting(T pawnSetting)
        {
            yield return new WaitForEndOfFrame();
            _setter(pawnSetting, null);
        }
    }
}
