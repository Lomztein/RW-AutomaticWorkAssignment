using Lomzie.AutomaticWorkAssignment.Source;
using Lomzie.AutomaticWorkAssignment.Source.UI.Windows;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class EditMapPawnFilterWindow : ListerWindow<Pawn>
    {
        private readonly MapWorkManager _workManager;
        private readonly MapPawnsFilter _filter;

        public override Vector2 InitialSize => new Vector2(650, 500);
        private float ListWidth => 350;

        public override string Header => "AWA.HeaderExcludedPawns".Translate();
        public override string AddButtonLabel => "AWA.ExcludePawn".Translate();

        private Texture2D _toggleOnIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
        private Texture2D _toggleOffIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

        public EditMapPawnFilterWindow(MapWorkManager workManager)
        {
            _workManager = workManager;
            _filter = workManager.MapPawnFilter;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            _filter.ExcludedPawns = _filter.ExcludedPawns.Where(x => x != null && x.Pawn != null).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            (Rect setting, Rect list) = Utils.SplitRectHorizontalRight(inRect, ListWidth);
            Widgets.DrawLineVertical(setting.x + setting.width, inRect.y, inRect.height);

            setting.width -= Margin;
            list.x += Margin;
            list.width -= Margin;

            base.DoWindowContents(list);
            DoSettingsContents(setting);

            if (_workManager == null)
            {
                Close();
            }
        }

        private void DoSettingsContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, 0);
            RectAggregator layout = new RectAggregator(rect, GetHashCode(), new Vector2(0, 0));

            float toggleSize = AutomaticWorkAssignmentSettings.UIButtonSizeBase;
            DoToggle(ref layout, ref _filter.IncludeColonists, toggleSize, "AWA.IncludeColonists".Translate());
            DoToggle(ref layout, ref _filter.IncludeGuests, toggleSize, "AWA.IncludeGuests".Translate());
            if (ModsConfig.IdeologyActive)
                DoToggle(ref layout, ref _filter.IncludeSlaves, toggleSize, "AWA.IncludeSlaves".Translate());
            DoToggle(ref layout, ref _filter.IncludePrisoners, toggleSize, "AWA.IncludePrisoners".Translate());

            DoToggle(ref layout, ref _filter.IncludeDowned, toggleSize, "AWA.IncludeDowned".Translate());
            DoToggle(ref layout, ref _filter.IncludeMentallyBroken, toggleSize, "AWA.IncludeMentallyBroken".Translate());
            DoToggle(ref layout, ref _filter.IncludeCryptosleepers, toggleSize, "AWA.IncludeCryptosleepers".Translate());

            layout.NewRow(Margin);
            Rect remainder = layout.NewRow(inRect.height - layout.Rect.height);
            if (_filter.IncludePrisoners)
            {
                Widgets.Label(remainder, "AWA.IncludePrisonersWarning".Translate());
            }
        }

        private void DoToggle(ref RectAggregator layout, ref bool toggle, float size, string label)
        {
            Rect row = layout.NewRow(size);
            (Rect labelRect, Rect toggleRect) = Utils.SplitRectHorizontalRight(row, row.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            if (Widgets.ButtonImage(toggleRect, toggle ? _toggleOnIcon : _toggleOffIcon))
                toggle = !toggle;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override IList<Pawn> GetList()
            => _filter.ExcludedPawns.Select(x => x.Pawn).ToList();

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
            Find.WindowStack.Add(new SearchableFloatMenu(GetFloatMenuOptions().ToList()));
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions ()
        {
            var pawns = _workManager.GetAllPawns().Where(x => !_filter.ExcludedPawns.Any(y => y.Is(x)));
            foreach (var pawn in pawns)
            {
                yield return new FloatMenuOption(pawn.Name.ToString(), () => _filter.ExcludedPawns.Add(new PawnRef(pawn)), pawn, Color.white);
            }
        }

        protected override void RemoveButtonClicked(Pawn element)
        {
            _filter.ExcludedPawns.RemoveAll(x => x.Is(element));
        }
    }
}
