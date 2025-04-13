using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class HediffPawnCondition : IPawnCondition
    {
        public string Label => "Health condition";
        public string Description => "Check if the pawn has the given health condition.";

        public HediffDef HediffDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref HediffDef, "hediffDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.health.hediffSet?.HasHediff(HediffDef) ?? false;
    }
}
