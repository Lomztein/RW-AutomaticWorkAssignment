using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DraftedPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.Drafted ?? false;
    }
}
