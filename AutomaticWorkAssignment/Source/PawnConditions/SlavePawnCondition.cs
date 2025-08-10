using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SlavePawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.IsSlaveOfColony;
    }
}
