using Lomzie.AutomaticWorkAssignment.UI.Modular;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source.UI.Modular
{
    public class PositionTargeter<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private static float ButtonSize => AutomaticWorkAssignmentSettings.UIButtonSizeBase;

        private readonly Func<T, Vector3?> _getter;
        private readonly Action<T, Vector3> _setter;

        public PositionTargeter(Func<T, Vector3?> getter, Action<T, Vector3> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect buttonRect = new Rect(position, new Vector2(width, ButtonSize));
            Vector3? cur = _getter(pawnSetting);
            if (Widgets.ButtonText(buttonRect, "AWA.PositionSelect".Translate()))
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForCell(), info => OnTargetSelected(info, pawnSetting), OnTargetUI);
            }
            if (Mouse.IsOver(buttonRect))
            {
                if (cur.HasValue) {
                    TargetHighlighter.Highlight(new GlobalTargetInfo(cur.Value.ToIntVec3(), Find.CurrentMap));
                }
            }
            return buttonRect.height;
        }

        private void OnTargetUI(LocalTargetInfo info)
        {
        }

        private void OnTargetSelected(LocalTargetInfo info, T pawnSetting)
        {
            if (info.IsValid)
            {
                _setter(pawnSetting, info.CenterVector3);
            }
        }
    }
}
