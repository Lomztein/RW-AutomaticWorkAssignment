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
    public class NestedPawnSettingUIHandler<TSetting, TNestedSetting, TSettingDef> : IPawnSettingUIHandler where TSetting : IPawnSetting where TNestedSetting : IPawnSetting where TSettingDef : PawnSettingDef
    {
        public virtual Action? HelpHandler(IPawnSetting setting) => setting.Def.documentationPath == null ?
            null :
            () => AutomaticWorkAssignmentMod.OpenWebDocumentation(setting.Def.documentationPath);
        public bool CanHandle(IPawnSetting pawnSetting)
            => pawnSetting is TSetting;

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
                    (TNestedSetting)pawnSetting.InnerSetting,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)),
                    (x, newSetting) => Find.Root.StartCoroutine(DelayedReplaceInnerSetting(pawnSetting, newSetting)));
            }
            else
            {
                WorkManagerWindow.DoAddSettingButton<TNestedSetting, TSettingDef>(ref layout, "AWA.NestedSettingSelect".Translate(), (newSetting) => pawnSetting.InnerSetting = newSetting, true);
            }

            return layout.Rect.height;
        }
        
        private IEnumerator DelayedRemoveInnerSetting(NestedPawnSetting setting)
        {
            yield return new WaitForEndOfFrame();
            setting.InnerSetting = null;
        }

        private IEnumerator DelayedReplaceInnerSetting(NestedPawnSetting setting, TNestedSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            setting.InnerSetting = newSetting;
        }
    }
}
