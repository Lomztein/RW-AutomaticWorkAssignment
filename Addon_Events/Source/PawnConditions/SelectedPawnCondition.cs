using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SelectedPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn != null && Find.Selector.SelectedPawns.Contains(pawn);
    }
}
