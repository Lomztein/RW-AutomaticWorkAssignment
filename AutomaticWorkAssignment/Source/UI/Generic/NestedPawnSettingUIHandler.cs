﻿using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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
                y += WorkManagerWindow.DoPawnSetting(
                    innerPosition,
                    innerWidth,
                    pawnSetting.InnerSetting,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)));
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

        private IEnumerable<D> GetDefs()
            => DefDatabase<D>.AllDefs;

        private IEnumerator DelayedRemoveInnerSetting(NestedPawnSetting setting)
        {
            yield return new WaitForEndOfFrame();
            setting.InnerSetting = null;
        }
    }
}
