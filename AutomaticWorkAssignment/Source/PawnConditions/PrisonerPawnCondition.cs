using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PrisonerPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                return pawn.IsPrisonerOfColony;
            }
            return false;
        }
    }
}
