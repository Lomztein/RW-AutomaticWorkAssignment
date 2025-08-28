using Lomzie.AutomaticWorkAssignment.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public class MultiplierPawnAmountUIHandler<T> : PawnAmountUIHandler<T> where T : IPawnAmount
    {
        private readonly Buffer<string> _buffer = new();

        private Func<T, float> _getter;
        private Action<T, float> _setter;

        public MultiplierPawnAmountUIHandler(Func<T, float> getter,  Action<T, float> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        protected override void Handle(Rect inRect, T pawnAmount)
        {
            string buffer = _buffer.Get(pawnAmount);
            Rect multLabelRect = Utils.GetSubRectFraction(inRect, new Vector2(0.75f, 0f), new Vector2(1f, 1f));
            Rect multRect = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0f), new Vector2(0.75f, 1f));
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(multLabelRect, "x");
            float value = _getter(pawnAmount);
            Widgets.TextFieldNumeric(multRect, ref value, ref buffer);
            _setter(pawnAmount, value);
            Text.Anchor = TextAnchor.UpperLeft;
            _buffer.Set(pawnAmount, buffer);
        }
    }
}
