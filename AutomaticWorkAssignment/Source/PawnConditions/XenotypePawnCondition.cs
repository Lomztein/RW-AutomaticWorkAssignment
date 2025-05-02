using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class XenotypePawnCondition : PawnSetting, IPawnCondition
    {
        public XenotypeDef XenotypeDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref XenotypeDef, "xenotypeDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => XenotypeDef != null && pawn.genes?.Xenotype == XenotypeDef;
    }
}
