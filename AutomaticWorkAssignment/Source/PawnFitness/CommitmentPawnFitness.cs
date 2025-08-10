using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class CommitmentPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return request.WorkManager.GetPawnCommitment(pawn);
            return 0;
        }
    }
}
