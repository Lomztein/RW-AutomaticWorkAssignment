using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AssignmentPawnCondition : PawnSetting, IPawnCondition
    {
        public int WorkSpecId = -1;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && WorkSpecId != -1)
            {
                if (request.WorkManager.TryGetSpecById(WorkSpecId, out var workSpec))
                {
                    return request.WorkManager.GetAssignmentTo(pawn, workSpec) != null;
                }
                else
                {
                    WorkSpecId = -1; // Reference lost somehow.
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref WorkSpecId, "workSpecId");
        }

        public override bool IsConfigured()
        {
            return WorkSpecId != -1;
        }
    }
}
