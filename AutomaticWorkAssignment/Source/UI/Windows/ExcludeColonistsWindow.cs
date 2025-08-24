using Lomzie.AutomaticWorkAssignment.Source.UI.Windows;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class ExcludeColonistsWindow : ListerWindow<Pawn>
    {
        private readonly MapWorkManager _workManager;

        public override string Header => "AWA.HeaderExcludedPawns".Translate();
        public override string AddButtonLabel => "AWA.ExcludePawn".Translate();

        public ExcludeColonistsWindow(MapWorkManager workManager)
        {
            _workManager = workManager;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            _workManager.ExcludedPawns = _workManager.ExcludedPawns.Where(x => x != null && x.Pawn != null).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            if (_workManager == null)
            {
                Close();
            }
        }

        public override IList<Pawn> GetList()
            => _workManager.ExcludedPawns.Select(x => x.Pawn).ToList();

        protected override void DrawRow(Rect inRect, Pawn element)
        {
            (Rect icon, Rect label) = Utils.SplitRectHorizontalLeft(inRect, inRect.height);
            label = Utils.ShrinkByMargin(label, 4);
            Widgets.ThingIcon(icon, element);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(label, element.Name.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        protected override void AddButtonClicked()
        {
            Find.WindowStack.Add(new FloatMenu(GetFloatMenuOptions().ToList()));
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions ()
        {
            var pawns = _workManager.GetAllPawns().Where(x => !_workManager.ExcludedPawns.Any(y => y.Is(x)));
            foreach (var pawn in pawns)
            {
                yield return new FloatMenuOption(pawn.Name.ToString(), () => _workManager.ExcludedPawns.Add(new PawnRef(pawn)), pawn, Color.white);
            }
        }

        protected override void RemoveButtonClicked(Pawn element)
        {
            _workManager.ExcludedPawns.RemoveAll(x => x.Is(element));
        }
    }
}
