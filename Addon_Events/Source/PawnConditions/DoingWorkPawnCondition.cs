using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DoingWorkPawnCondition : PawnSetting, IPawnCondition
    {
        public WorkTypeDef WorkTypeDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.jobs != null && pawn.jobs.jobQueue != null)
            {
                Job job = pawn.jobs.AllJobs().FirstOrDefault(x => x.def != JobDefOf.Goto);
                if (job != null && job.workGiverDef != null)
                    return WorkTypeDef != null && job.workGiverDef.workType == WorkTypeDef;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref WorkTypeDef, "workTypeDef");
        }
    }
}
