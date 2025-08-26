using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DoingWorkPawnCondition : PawnSetting, IPawnCondition
    {
        public WorkTypeDef WorkTypeDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => WorkTypeDef != null && pawn?.CurJob?.workGiverDef?.workType == WorkTypeDef;
    }
}
