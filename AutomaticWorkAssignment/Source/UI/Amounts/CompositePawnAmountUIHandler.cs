using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public class CompositePawnAmountUIHandler<T> : PawnAmountUIHandler<T> where T : IPawnAmount
    {
        private Func<T, IList<IPawnAmount>> _innerGetter;
        private string _label;

        public CompositePawnAmountUIHandler(Func<T, IList<IPawnAmount>> innerGetter, string label) 
        {
            _innerGetter = innerGetter;
            _label = label;
        }

        protected override void Handle(Rect inRect, T pawnAmount)
        {
            IList<IPawnAmount> inner = _innerGetter(pawnAmount);

            string innerString = inner.NullOrEmpty() ? 
                "AWA.Empty".Translate() : 
                string.Join(", ", inner.Select(x => DefDatabase<PawnAmountDef>.AllDefs.First(y => y.defClass == x.GetType()).icon));

            if (Widgets.ButtonText(inRect, _label.Translate(innerString)))
            {
                Find.WindowStack.Add(new EditCompositePawnAmountWindow(inner));
            }
        }
    }
}
