using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class HealthPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.health.summaryHealth.SummaryHealthPercent ?? 0;
    }
}
