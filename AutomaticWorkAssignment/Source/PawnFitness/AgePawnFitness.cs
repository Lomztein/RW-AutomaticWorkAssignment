using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class AgePawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return pawn.ageTracker.AgeBiologicalYearsFloat;
            return 0f;
        }
    }
}
