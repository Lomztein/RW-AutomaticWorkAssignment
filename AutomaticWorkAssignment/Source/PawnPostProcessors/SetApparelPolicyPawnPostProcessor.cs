using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetApparelPolicyPawnPostProcessor : IPawnPostProcessor
    {
        public string Label => "Set apparel policy";
        public string Description => "Set the pawns apparel policy on assignment";

        public ApparelPolicy Policy;

        public void ExposeData()
        {
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
