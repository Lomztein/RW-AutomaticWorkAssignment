using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class InspiredPawnCondition : PawnSetting, IPawnCondition
    {
        public InspirationDef InspirationDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return false;

            if (InspirationDef == null)
                return pawn.Inspired;
            return pawn.Inspired && pawn.InspirationDef == InspirationDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref InspirationDef, "inspirationDef");
        }
    }
}
