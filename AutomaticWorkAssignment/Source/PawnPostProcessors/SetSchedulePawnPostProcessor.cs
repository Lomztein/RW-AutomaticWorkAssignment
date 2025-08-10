using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetSchedulePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public List<TimeAssignmentDef> Times;

        public SetSchedulePawnPostProcessor()
        {
            Times = Enumerable.Range(0, GenDate.HoursPerDay).Select(x => TimeAssignmentDefOf.Anything).ToList();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Times, "times", LookMode.Def);
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            for (int i = 0; i < Times.Count; i++)
            {
                pawn.timetable.SetAssignment(i, Times[i]);
            }
        }
    }
}
