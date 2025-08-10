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
        private float AddButtonSIze => AutomaticWorkAssignmentSettings.UIButtonSizeBase;

        private readonly Func<T, IPawnSetting> _getter;
        private readonly Action<T, IPawnSetting> _setter;

        public Nested(Func<T, IPawnSetting> getter, Action<T, IPawnSetting> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            float y = 0f;
            Vector2 innerPosition = position;

            innerPosition.x += 4;
            float innerWidth = width - 4;
            IPawnSetting current = _getter(pawnSetting);

            if (current != null)
            {
                y += WorkManagerWindow.DoPawnSetting(
                    innerPosition,
                    innerWidth,
                    current,
                    canMoveUp: false,
                    canMoveDown: false,
                    null,
                    (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(pawnSetting)));
            }
            else
            {
                Rect addSettingBUttonRect = new Rect(innerPosition, new Vector2(innerWidth, AddButtonSIze));
                if (Widgets.ButtonText(addSettingBUttonRect, "AWA.NestedSettingSelect".Translate()))
                {
                    FloatMenuUtility.MakeMenu(GetDefs(), x => x.LabelCap, x => () => _setter(pawnSetting, PawnSetting.CreateFrom<IPawnSetting>(x)));
                }

                y += addSettingBUttonRect.height;
            }

            return y;
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
