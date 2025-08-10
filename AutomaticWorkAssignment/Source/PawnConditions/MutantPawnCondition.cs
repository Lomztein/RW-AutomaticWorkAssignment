using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class MutantPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn.IsMutant;
        }
    }
}
