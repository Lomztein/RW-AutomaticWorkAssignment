using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.UI
{
    public class NestedPawnSettingUIHandler<T, D> : IPawnSettingUIHandler where T : IPawnSetting where D : PawnSettingDef
    {
        public virtual Action? GetHelp => null;
        private readonly float _addConditionButtonSize = 32;

        public bool CanHandle(IPawnSetting pawnSetting)
            => pawnSetting is T;

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting)
            => Handle(position, width, (NestedPawnSetting)pawnSetting);

        protected float Handle(Vector2 position, float width, NestedPawnSetting pawnSetting)
        {
            const int inset = 8;
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: inset), GetHashCode(), new(0, 0));

            if (pawnSetting.InnerSetting != null)
            {
                WorkManagerWindow.DoPawnSetting(
                    ref layout,
                    pawnSetting.InnerSetting,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)));
            }
            else
            {
                Rect addConditionButtonRect = layout.NewRow(_addConditionButtonSize);
                if (Widgets.ButtonText(addConditionButtonRect, "AWA.NestedSettingSelect".Translate()))
                {
                    FloatMenuUtility.MakeMenu(GetDefs(), x => x.LabelCap, x => () => pawnSetting.InnerSetting = PawnSetting.CreateFrom<IPawnSetting>(x));
                }
            }

            return layout.Rect.height;
        }

        private IEnumerable<D> GetDefs()
            => DefDatabase<D>.AllDefs;

        private IEnumerator DelayedRemoveInnerSetting(NestedPawnSetting setting)
        {
            yield return new WaitForEndOfFrame();
            setting.InnerSetting = null;
        }
    }
}
