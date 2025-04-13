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
    public class SetFoodPolicyPawnPostProcessor : IPawnPostProcessor
    {
        public string Label => "Set food policy";
        public string Description => "Set the pawns food policy on assignment";

        public FoodPolicy Policy;

        public void ExposeData()
        {
            Scribe_References.Look(ref Policy, "policy");
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (Policy != null)
            {
                pawn.foodRestriction.CurrentFoodPolicy = Policy;
            }
        }
    }
}
