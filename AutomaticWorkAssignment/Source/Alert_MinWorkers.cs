using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Alert_MinWorkers : Alert
    {
        private List<Tuple<WorkSpecification, Map>> _specsWithIssue = new List<Tuple<WorkSpecification, Map>>();
        private bool RedCriticalAlert => AutomaticWorkAssignmentSettings.RedCriticalAlert;

        private bool AnyIssues => _specsWithIssue.Any();
        private bool AnyCritical => _specsWithIssue.Any(x => x.Item1.IsCritical);
        public bool DisplayRedCritical => AnyCritical && RedCriticalAlert;

        private Cache<AlertReport> _reportCache;

        public override AlertReport GetReport()
        {
            if (_reportCache == null)
                _reportCache = new Cache<AlertReport>(CacheReport);

            return _reportCache.Get();
        }

        private AlertReport CacheReport()
        {
            _specsWithIssue.Clear();
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                MapWorkManager manager = MapWorkManager.GetManager(map);
                foreach (WorkSpecification workSpec in manager.WorkList)
                {
                    if (workSpec.EnableAlert)
                    {
                        if (!manager.CanWorkSpecificationBeMinimallySatisfiedWithApplicablePawns(workSpec, manager.MakeDefaultRequest()))
                        {
                            _specsWithIssue.Add(new Tuple<WorkSpecification, Map>(workSpec, map));
                        }
                    }
                }
            }

            return AnyIssues ? AlertReport.Active : AlertReport.Inactive;
        }

        private List<Tuple<WorkSpecification, Map>> GetSpecsWithIssuesSorted()
        {
            List<Tuple<WorkSpecification, Map>> specs = new List<Tuple<WorkSpecification, Map>>(_specsWithIssue);
            specs.Sort(Compare);
            return specs;
        }

        private int Compare(Tuple<WorkSpecification, Map> lhs, Tuple<WorkSpecification, Map> rhs)
        {
            int first = lhs.Item1.IsCritical ? -1 : 1;
            int second = rhs.Item1.IsCritical ? -1 : 1;
            return first - second;
        }

        protected override Color BGColor => GetColor();

        public override string GetLabel()
            => AnyCritical ? "AWA.CriticalWorkUnsatisfied".Translate() : "AWA.WorkUnsatisfied".Translate();

        public override TaggedString GetExplanation()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"AWA.WorkAlertExplanation".Translate());

            var byMap = _specsWithIssue.GroupBy(x => x.Item2);
            foreach (var kvp in byMap)
            {
                var map = kvp.Key;
                builder.AppendLine();
                if (map.Parent != null)
                {
                    builder.AppendLine(map.Parent.LabelCap + ":");
                }
                foreach (var tuple in kvp)
                {
                    var spec = tuple.Item1;
                    builder.Append("  ");
                    string line = spec.IsCritical ? $"<b>{spec.Name}</b>" : spec.Name;
                    builder.AppendLine(line);
                }
            }

            return builder.ToString();
        }

        private Color GetColor()
        {
            if (DisplayRedCritical)
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.red;
            }
            else
            {
                return Color.clear;
            }
        }

        public override AlertPriority Priority => DisplayRedCritical ? AlertPriority.Critical : AlertPriority.Medium;

        protected override void OnClick()
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.AllDefs.First(x => x.tabWindowClass == typeof(WorkManagerWindow));
            Find.MainTabsRoot.SetCurrentTab(def);
            var moveTo = GetSpecsWithIssuesSorted().FirstOrDefault(x => x.Item2 == Current.Game.CurrentMap);
            if (moveTo == null)
            {
                moveTo = GetSpecsWithIssuesSorted().First();
            }
            Current.Game.CurrentMap = moveTo.Item2;
            (def.TabWindow as WorkManagerWindow).SetCurrentWorkSpecification(moveTo.Item1);
        }
    }
}
