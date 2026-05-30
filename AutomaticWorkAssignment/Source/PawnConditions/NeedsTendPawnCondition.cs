using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class NeedsTendPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn != null && pawn.health != null && pawn.health.HasHediffsNeedingTend(false);
        }
    }
}
