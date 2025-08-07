using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class CapacityPawnFitness : PawnSetting, IPawnFitness
    {
        public PawnCapacityDef CapacityDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => CapacityDef != null ? (pawn?.health.capacities?.GetLevel(CapacityDef) ?? 0): 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref CapacityDef, "capacityDef");
        }
    }
}
