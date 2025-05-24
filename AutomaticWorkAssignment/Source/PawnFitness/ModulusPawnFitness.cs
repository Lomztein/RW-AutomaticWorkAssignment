using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ModulusPawnFitness : NestedPawnSetting, IPawnFitness
    {
        public int Modulus;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return (InnerSetting as IPawnFitness).CalcFitness(pawn, specification, request) % Modulus;
        }
    }
}
