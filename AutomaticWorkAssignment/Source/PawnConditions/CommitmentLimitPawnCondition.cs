using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class CommitmentLimitPawnCondition : PawnSetting, IPawnCondition
    {
        public float Limit = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Limit, "Limit");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return request.WorkManager.GetPawnCommitment(pawn) < Limit;
            return false;
        }
    }
}
