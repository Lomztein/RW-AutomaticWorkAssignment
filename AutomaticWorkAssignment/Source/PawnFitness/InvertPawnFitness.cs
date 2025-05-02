using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    internal class InvertPawnFitness : NestedPawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSetting != null && InnerSetting is IPawnFitness innerFitness)
            {
                return -innerFitness.CalcFitness(pawn, specification, request);
            }
            return 0f;
        }
    }
}
