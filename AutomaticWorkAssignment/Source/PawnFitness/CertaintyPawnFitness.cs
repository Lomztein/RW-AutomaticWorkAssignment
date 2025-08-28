using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class CertaintyPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Ideo != null)
                return pawn.ideo.Certainty;
            return 0f;
        }
    }
}
