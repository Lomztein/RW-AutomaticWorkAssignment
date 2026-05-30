using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public class CountPawnAmountUIHandler : PawnAmountUIHandler<CountPawnAmount>
    {
        private readonly Buffer<string> _buffer = new();

        protected override void Handle(Rect inRect, CountPawnAmount pawnAmount)
        {
            string buffer = _buffer.Get(pawnAmount);
            Rect pickerRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(0.7f, 1f));
            Rect multLabelRect = Utils.GetSubRectFraction(inRect, new Vector2(0.7f, 0f), new Vector2(0.8f, 1f));
            Rect multRect = Utils.GetSubRectFraction(inRect, new Vector2(0.8f, 0f), new Vector2(1f, 1f));

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(multLabelRect, "x");
            Widgets.TextFieldNumeric(multRect, ref pawnAmount.Multiplier, ref buffer);
            Text.Anchor = TextAnchor.UpperLeft;

            List<IPawnCondition> conditions = pawnAmount.PawnConditions;

            string innerString = conditions.NullOrEmpty() ? 
                "AWA.Empty".Translate() : 
                string.Join(", ", conditions.Select(x => x.Label.First()));

            if (Widgets.ButtonText(pickerRect, "AWA.ConditionsEdit".Translate(innerString)))
            {
                Find.WindowStack.Add(new EditPawnSettingsListWindow<IPawnCondition>(conditions));
            }

            _buffer.Set(pawnAmount, buffer);
        }
    }
}
