using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Alert_MinWorkers : Alert
    {
        private List<WorkSpecification> _specsWithIssue = new List<WorkSpecification>();

        private bool AnyIssues => _specsWithIssue.Any();
        private bool AnyCritical => _specsWithIssue.Any(x => x.IsCritical);

        public override AlertReport GetReport()
        {
            _specsWithIssue.Clear();
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                MapWorkManager manager = MapWorkManager.GetManager(map);
                foreach (WorkSpecification workSpec in manager.WorkList)
                {
                    if (!manager.CanWorkSpecificationBeMinimallySatisfiedWithApplicablePawns(workSpec, manager.MakeDefaultRequest()))
                    {
                        _specsWithIssue.Add(workSpec);
                    }
                }
            }

            return AnyIssues ? AlertReport.Active : AlertReport.Inactive;
        }

        private List<WorkSpecification> GetSpecsWithIssuesSorted ()
        {
            List<WorkSpecification> specs = new List<WorkSpecification>(_specsWithIssue);
            specs.Sort(Compare);
            return specs;
        }

        private int Compare(WorkSpecification lhs, WorkSpecification rhs)
        {
            int first = lhs.IsCritical ? 1 : -1;
            int second = rhs.IsCritical ? 1 : -1;
            return first - second;
        }

        protected override Color BGColor => GetColor();

        public override string GetLabel()
        {
            if (AnyCritical)
                return "AWA.CriticalWorkUnsatisfied".Translate();
            return "Work unsatisfied".Translate();
        }

        public override TaggedString GetExplanation()
            => $"AWA.WorkAlertExplanation".Translate().Formatted(string.Join(", ", GetSpecsWithIssuesSorted().Select(x => x.Name)));

        private Color GetColor ()
        {
            if (AnyCritical)
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.red;
            }
            else
            {
                return Color.clear;
            }
        }

        public override AlertPriority Priority => AnyCritical ? AlertPriority.Critical : AlertPriority.Medium;

        protected override void OnClick()
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.AllDefs.First(x => x.tabWindowClass == typeof(WorkManagerWindow));
            Find.MainTabsRoot.SetCurrentTab(def);
            (def.TabWindow as WorkManagerWindow).SetCurrent(GetSpecsWithIssuesSorted().First());
        }
    }
}
