using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetSchedulePawnPostProcessor : IPawnPostProcessor
    {
        public string Label => "Set schedule";
        public string Description => "Set pawn schedule on assignment.";

        public List<TimeAssignmentDef> Times;

        public SetSchedulePawnPostProcessor()
        {
            Times = Enumerable.Range(0, GenDate.HoursPerDay).Select(x => TimeAssignmentDefOf.Anything).ToList();
        }

        public void ExposeData()
        {
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
