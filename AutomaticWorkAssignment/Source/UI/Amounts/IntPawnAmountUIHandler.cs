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
    public class IntPawnAmountUIHandler : PawnAmountUIHandler<IntPawnAmount>
    {
        private readonly Buffer<string> _buffer = new();

        protected override void Handle(Rect inRect, IntPawnAmount pawnSetting)
        {
            string buffer = _buffer.Get(pawnSetting);
            Widgets.TextFieldNumeric(inRect, ref pawnSetting.Value, ref buffer);
            _buffer.Set(pawnSetting, buffer);
        }
    }
}
