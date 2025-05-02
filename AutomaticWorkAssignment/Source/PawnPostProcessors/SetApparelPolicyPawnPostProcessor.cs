using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetApparelPolicyPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public ApparelPolicy Policy;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Policy, "policy");
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (Policy != null) { 
                pawn.outfits.CurrentApparelPolicy = Policy;
            }
        }
    }
}
