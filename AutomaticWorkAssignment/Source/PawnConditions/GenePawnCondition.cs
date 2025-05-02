using AutomaticWorkAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class GenePawnCondition : PawnSetting, IPawnCondition
    {
        public GeneDef GeneDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref GeneDef, "geneDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.genes.HasActiveGene(GeneDef);
    }
}
