using Lomzie.AutomaticWorkAssignment.Defs;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Nested<TSetting, TNestedSetting, TSettingDef> : IHandlerModule<TSetting> where TSetting : IPawnSetting where TNestedSetting : IPawnSetting where TSettingDef : PawnSettingDef
    {
        private readonly Func<TSetting, TNestedSetting> _getter;
        private readonly Action<TSetting, TNestedSetting> _setter;
        private string _selectLabel;

        public Nested(Func<TSetting, TNestedSetting> getter, Action<TSetting, TNestedSetting> setter, string selectLabel = null)
        {
            _getter = getter;
            _setter = setter;
            _selectLabel = selectLabel ?? "AWA.NestedSettingSelect".Translate();
        }

        public float Handle(Vector2 position, float width, TSetting pawnSetting)
        {
            const int inset = 8;
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: inset), GetHashCode(), new(0, 1));
            TNestedSetting current = _getter(pawnSetting);

            if (current != null)
            {
                WorkManagerWindow.DoPawnSetting(
                    ref layout,
                    current,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)),
                    (x, newSetting) => Find.Root.StartCoroutine(DelayedReplaceInnerSetting(pawnSetting, newSetting)));
            }
            else
            {
                WorkManagerWindow.DoAddSettingButton<TNestedSetting, TSettingDef>(ref layout, _selectLabel, (newSetting) => _setter(pawnSetting, newSetting), true);
            }

            return layout.Rect.height;
        }

        private IEnumerator DelayedRemoveInnerSetting(TSetting pawnSetting)
        {
            yield return new WaitForEndOfFrame();
            _setter(pawnSetting, default);
        }

        private IEnumerator DelayedReplaceInnerSetting(TSetting pawnSetting, TNestedSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            _setter(pawnSetting, newSetting);
        }
    }
}
