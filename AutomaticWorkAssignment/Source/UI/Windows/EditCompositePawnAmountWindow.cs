using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.Source.UI.Windows;
using Lomzie.AutomaticWorkAssignment.UI.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class EditCompositePawnAmountWindow : ListerWindow<IPawnAmount>
    {
        public override Vector2 InitialSize => new Vector2(300, 400);

        public override string Header => "AWA.CompositeAmountEdit".Translate();
        public override string AddButtonLabel => "AWA.AmountAdd".Translate();

        private readonly IList<IPawnAmount> _amounts;

        public EditCompositePawnAmountWindow(IList<IPawnAmount> amounts)
        {
            _amounts = amounts;
        }

        public override IList<IPawnAmount> GetList()
            => _amounts;

        protected override void AddButtonClicked()
        {
            var list = GetList();
            SearchableFloatMenu.MakeMenu(DefDatabase<PawnAmountDef>.AllDefs, x => x.LabelCap, x => () => list.Add((IPawnAmount)Activator.CreateInstance(x.defClass)));
        }

        protected override void DrawRow(Rect inRect, IPawnAmount element)
        {
            (Rect label, Rect content) = Utils.SplitRectHorizontalLeft(inRect, inRect.height);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(label, DefDatabase<PawnAmountDef>.AllDefs.First(x => x.defClass == element.GetType()).icon);
            Text.Anchor = TextAnchor.UpperLeft;
            PawnAmountUIHandlers.Handle(content, element);
        }

        protected override void RemoveButtonClicked(IPawnAmount element)
        {
            GetList().Remove(element);
        }
    }
}
