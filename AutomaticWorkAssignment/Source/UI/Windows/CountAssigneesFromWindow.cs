using Lomzie.AutomaticWorkAssignment.Source.UI.Windows;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class CountAssigneesFromWindow : ListerWindow<WorkSpecification>
    {
        public override string Header => "AWA.LabelCountAssigneesFrom".Translate();
        public override string AddButtonLabel => "AWA.LabelAddWorkSpecification".Translate();

        private readonly WorkSpecification _workSpecification;
        private readonly MapWorkManager _workManager;

        public CountAssigneesFromWindow(WorkSpecification workSpecification, MapWorkManager workManager)
        {
            _workManager = workManager;
            _workSpecification = workSpecification;
        }

        public override IList<WorkSpecification> GetList()
            => _workSpecification.CountAssigneesFrom;

        protected override void AddButtonClicked()
        {
            var canAdd = _workManager.WorkList.Where(x => _workManager.AllowCountAssigneesFrom(_workSpecification, x) && !_workSpecification.CountAssigneesFrom.Contains(x)).ToList();
            FloatMenuUtility.MakeMenu(canAdd, x => x.Name, x => () => _workSpecification.CountAssigneesFrom.Add(x));
        }

        protected override void DrawRow(Rect inRect, WorkSpecification element)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(inRect, element.Name);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        protected override void RemoveButtonClicked(WorkSpecification element)
        {
            _workSpecification.CountAssigneesFrom.Remove(element);
        }
    }
}
