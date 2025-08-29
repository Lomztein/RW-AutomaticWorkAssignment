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
    public class ThingTargeter<T> : IHandlerModule<T> where T : IPawnSetting
    {
        private static float ButtonSize => AutomaticWorkAssignmentSettings.UIButtonSizeBase;

        private readonly Func<T, Thing> _getter;
        private readonly Action<T, Thing> _setter;
        private readonly Predicate<Thing> _allowTarget;

        private readonly Func<T, string> _labelGetter;

        public ThingTargeter(Func<T, Thing> getter, Action<T, Thing> setter, Predicate<Thing> allowTarget, Func<T, string> labelGetter)
        {
            _getter = getter;
            _setter = setter;
            _allowTarget = allowTarget;
            _labelGetter = labelGetter;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect buttonRect = new Rect(position, new Vector2(width, ButtonSize));
            Thing cur = _getter(pawnSetting);
            if (Widgets.ButtonText(buttonRect, _labelGetter(pawnSetting)))
            {
                var targetingParameters = TargetingParameters.ForThing();
                targetingParameters.validator = (x) => x.Thing != null && _allowTarget(x.Thing);
                Find.Targeter.BeginTargeting(targetingParameters, info => OnTargetSelected(info, pawnSetting), OnTargetUI);
            }
            if (Mouse.IsOver(buttonRect))
            {
                if (cur != null) {
                    TargetHighlighter.Highlight(new GlobalTargetInfo(cur.Position, Find.CurrentMap));
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
                _setter(pawnSetting, info.Thing);
            }
        }
    }
}
