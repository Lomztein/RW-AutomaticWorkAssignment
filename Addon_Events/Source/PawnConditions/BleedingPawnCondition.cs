using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class BleedingPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.health.hediffSet.BleedRateTotal > 0;
    }
}
