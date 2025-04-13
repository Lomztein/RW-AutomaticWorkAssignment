using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class TraitPawnCondition : IPawnCondition
    {
        public string Label => "Trait";
        public string Description => "Check if pawn has a given trait.";

        public TraitDef TraitDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref TraitDef, "traitDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.story?.traits?.HasTrait(TraitDef) ?? false;
    }
}
