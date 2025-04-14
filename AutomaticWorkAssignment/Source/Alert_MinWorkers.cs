using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Alert_MinWorkers : Alert
    {
        private WorkSpecification _specWithIssue;
        private bool _critical;

        public override AlertReport GetReport()
        {
            bool anyIssues = false;
            bool anyCritical = false;

            foreach (WorkSpecification workSpec in WorkManager.Instance.WorkList)
            {
                if (!WorkManager.Instance.IsWorkSpecificationMinimallySatisfied(workSpec))
                {
                    anyIssues = true;
                    if (workSpec.IsCritical)
                    {
                        anyCritical = true;
                        _specWithIssue = workSpec;
                        break; // Break when critical is found, ensuring that it is marked as the one with an issue.
                    }
                    _specWithIssue = workSpec;
                }
            }

            if (anyIssues)
            {
                _critical = anyCritical;
                return AlertReport.Active;
            }

            _specWithIssue = null;
            return AlertReport.Inactive;
        }

        protected override Color BGColor => GetColor();

        public override string GetLabel()
        {
            if (_critical)
                return "Critical work unsatisfied!";
            return "Work unsatisfied";
        }

        public override TaggedString GetExplanation()
            => $"Work specification '{_specWithIssue.Name}' is not minimally satisfied.";

        private Color GetColor ()
        {
            if (_critical)
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.red;
            }
            else
            {
                return Color.clear;
            }
        }

        public override AlertPriority Priority => _critical ? AlertPriority.Critical : AlertPriority.Medium;

        protected override void OnClick()
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.AllDefs.First(x => x.tabWindowClass == typeof(WorkManagerWindow));
            Find.MainTabsRoot.SetCurrentTab(def);
            (def.TabWindow as WorkManagerWindow).SetCurrent(_specWithIssue);
        }
    }
}
