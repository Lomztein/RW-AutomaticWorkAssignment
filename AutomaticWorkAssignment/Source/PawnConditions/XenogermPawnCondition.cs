using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class XenogermPawnCondition : PawnSetting, IPawnCondition
    {
        public CustomXenogerm Xenogerm;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Xenogerm, "xenogerm");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && Xenogerm != null)
            {
                return pawn.genes?.xenotypeName?.ToLowerInvariant() == Xenogerm.name.ToLowerInvariant();
            }
            return false;
        }
    }
}
