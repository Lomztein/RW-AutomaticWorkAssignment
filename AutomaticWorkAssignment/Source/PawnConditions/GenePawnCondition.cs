using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class GenePawnCondition : IPawnCondition
    {
        public string Label => "Has gene";
        public string Description => "Check if the pawn has the given gene.";

        public GeneDef GeneDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref GeneDef, "geneDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.genes.HasActiveGene(GeneDef);
    }
}
