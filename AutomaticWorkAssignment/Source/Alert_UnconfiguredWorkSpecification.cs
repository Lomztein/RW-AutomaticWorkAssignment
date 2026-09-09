using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Alert_UnconfiguredWorkSpecification : Alert
    {
        private List<Tuple<WorkSpecification, IPawnSetting, Map>> _specsWithIssue = new();
        private Cache<AlertReport> _reportCache = new();

        public override AlertReport GetReport()
        {
            if (_reportCache.TryGet(out var report))
                return report;
            report = CacheReport();
            _reportCache.Set(report);
            return report;
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
                    IEnumerable<IPawnSetting> allSettings =
                        Enumerable.Concat<IPawnSetting>(workSpec.Fitness, workSpec.Conditions).Concat(workSpec.PostProcessors);
                    foreach (IPawnSetting setting in allSettings)
                    {
                        if (setting != null)
                        {
                            if (!setting.IsConfigured())
                            {
                                _specsWithIssue.Add(new Tuple<WorkSpecification, IPawnSetting, Map>(workSpec, setting, map));
                            }
                        }
                    }
                }
            }

            return _specsWithIssue.Count > 0 ? AlertReport.Active : AlertReport.Inactive;
        }

        public override string GetLabel()
        {
            return "AWA.UnconfiguredWorkSpecificationLabel".Translate();
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder sb = new("AWA.UnconfiguredWorkSpecificationExplanationHeader".Translate());
            sb.AppendLine();
            foreach (var item in _specsWithIssue)
            {
                string mapName = item.Item3.Parent?.Label ?? "AWA.UnknownMap".Translate();
                string specName = item.Item1.Name;
                string settingName = item.Item2.Label;
                sb.AppendLine($"  {mapName}: {specName} - {settingName}");
            }
            return sb.ToString();
        }

        protected override void OnClick()
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.AllDefs.First(x => x.tabWindowClass == typeof(WorkManagerWindow));
            Find.MainTabsRoot.SetCurrentTab(def);
            var moveTo = _specsWithIssue.FirstOrDefault(x => x.Item3 == Current.Game.CurrentMap);
            if (moveTo == null)
            {
                moveTo = _specsWithIssue.First();
            }
            Current.Game.CurrentMap = moveTo.Item3;
            WorkManagerWindow.SetCurrentWorkSpecification(moveTo.Item1);
        }
    }
}
