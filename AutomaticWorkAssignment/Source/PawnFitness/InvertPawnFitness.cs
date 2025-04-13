using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    internal class InvertPawnFitness : NestedPawnSetting, IPawnFitness
    {
        public override string Label => "Invert";
        public override string Description => "Invert the fitness, so that positive becomes negative.";

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => -((IPawnFitness)InnerSetting).CalcFitness(pawn, specification, request);
    }
}
