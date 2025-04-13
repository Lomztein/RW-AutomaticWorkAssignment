using Lomzie.AutomaticWorkAssignment.PawnConditions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class XenotypePawnCondition : IPawnCondition
    {
        public string Label => "Xenotype";

        public string Description => "Check if a pawn is of the given xenotype";

        public XenotypeDef XenotypeDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref XenotypeDef, "xenotypeDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => XenotypeDef != null && pawn.genes?.Xenotype == XenotypeDef;
    }
}
