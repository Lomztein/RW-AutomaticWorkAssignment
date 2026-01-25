using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class TimeAssignmentCondition : PawnSetting, IPawnCondition
    {
        public TimeAssignmentDef TimeAssignmentDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref TimeAssignmentDef, "timeAssignmentDef");
        }
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.timetable.CurrentAssignment == TimeAssignmentDef;
    }
}
