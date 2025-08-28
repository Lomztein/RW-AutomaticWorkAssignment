using Lomzie.AutomaticWorkAssignment.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public class BuildingPawnAmountUIHandler : PawnAmountUIHandler<BuildingPawnAmount>
    {
        private readonly Buffer<string> _buffer = new();

        protected override void Handle(Rect inRect, BuildingPawnAmount pawnAmount)
        {
            string buffer = _buffer.Get(pawnAmount);
            Rect pickerRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(0.7f, 1f));
            Rect multLabelRect = Utils.GetSubRectFraction(inRect, new Vector2(0.7f, 0f), new Vector2(0.8f, 1f));
            Rect multRect = Utils.GetSubRectFraction(inRect, new Vector2(0.8f, 0f), new Vector2(1f, 1f));
            if (Widgets.ButtonText(pickerRect, pawnAmount.BuildingDef?.LabelCap ?? "AWA.Select".Translate()))
            {
                IEnumerable<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.BuildableByPlayer);
                SearchableFloatMenu.MakeMenu(defs, x => x.label, x => () => pawnAmount.BuildingDef = x);
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(multLabelRect, "x");
            Widgets.TextFieldNumeric(multRect, ref pawnAmount.Multiplier, ref buffer);
            Text.Anchor = TextAnchor.UpperLeft;
            _buffer.Set(pawnAmount, buffer);
        }
    }
}
