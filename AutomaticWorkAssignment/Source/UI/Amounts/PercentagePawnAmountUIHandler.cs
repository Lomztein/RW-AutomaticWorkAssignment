using Lomzie.AutomaticWorkAssignment.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public class PercentagePawnAmountUIHandler : PawnAmountUIHandler<PercentagePawnAmount>
    {
        private readonly Buffer<string> _buffer = new();

        protected override void Handle(Rect inRect, PercentagePawnAmount pawnSetting)
        {
            string buffer = _buffer.Get(pawnSetting);
            float percentage = pawnSetting.Percentage * 100f;
            Widgets.TextFieldNumeric(inRect, ref percentage, ref buffer, 0f, 100f);
            pawnSetting.Percentage = percentage / 100f;
            _buffer.Set(pawnSetting, buffer);
        }
    }
}
