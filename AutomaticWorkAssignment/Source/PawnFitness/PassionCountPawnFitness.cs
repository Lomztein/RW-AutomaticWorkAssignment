using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class PassionCountPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                return pawn?.skills.skills.Count(x => Utils.HasPassionIn(pawn, x.def)) ?? 0;
            }
            return 0f;
        }
    }
}
