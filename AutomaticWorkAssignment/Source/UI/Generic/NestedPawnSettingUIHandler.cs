using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace AutomaticWorkAssignment.UI
{
    public class NestedPawnSettingUIHandler<T, D> : IPawnSettingUIHandler where T : IPawnSetting where D : PawnSettingDef
    {
        private readonly float _addConditionButtonSize = 32;
        private readonly float _labelSize = 24;

        public bool CanHandle(IPawnSetting pawnSetting)
            => pawnSetting is T;

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting)
            => Handle(position, width, (NestedPawnSetting)pawnSetting);

        protected float Handle(Vector2 position, float width, NestedPawnSetting pawnSetting)
        {
            float y = 0f;
            Vector2 innerPosition = position;

            innerPosition.x += 4;
            float innerWidth = width - 4;
            if (pawnSetting.InnerSetting != null)
            {
                Rect labelRect = new Rect(innerPosition.x, position.y, innerWidth, _labelSize);
                string label = pawnSetting.InnerSetting?.Label ?? "AWA.NestedSettingNoneSelected".Translate();
                Widgets.Label(labelRect, pawnSetting.InnerSetting.Label);

                Rect deleteButtonRect = new Rect(innerPosition.x + innerWidth - _labelSize, position.y, _labelSize, _labelSize);
                if (Widgets.ButtonText(deleteButtonRect, "X"))
                {
                    Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting));
                }

                innerPosition.y += _labelSize;
                y += _labelSize;

                y += PawnSettingUIHandlers.Handle(innerPosition, innerWidth, pawnSetting.InnerSetting);
            }
            else
            {
                Rect addConditionButtonRect = new Rect(innerPosition, new Vector2(innerWidth, _addConditionButtonSize));
                if (Widgets.ButtonText(addConditionButtonRect, "AWA.NestedSettingSelect".Translate()))
                {
                    FloatMenuUtility.MakeMenu(GetDefs(), x => x.LabelCap, x => () => pawnSetting.InnerSetting = PawnSetting.CreateFrom<IPawnSetting>(x));
                }

                y += addConditionButtonRect.height;
            }

            return y;
        }

        private IEnumerable<D> GetDefs ()
            => DefDatabase<D>.AllDefs;

        private IEnumerator DelayedRemoveInnerSetting (NestedPawnSetting setting)
        {
            yield return new WaitForEndOfFrame();
            setting.InnerSetting = null;
        }
    }
}
