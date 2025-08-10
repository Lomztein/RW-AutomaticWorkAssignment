using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class IdeoligionPawnCondition : PawnSetting, IPawnCondition
    {
        public Ideo Ideoligion;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return pawn.Ideo == Ideoligion;
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Ideoligion, "ideo");
        }
    }
}
