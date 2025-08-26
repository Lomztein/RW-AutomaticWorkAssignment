using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DraftPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public bool Value;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.drafter.Drafted != Value)
                pawn.drafter.Drafted = Value;
        }
    }
}
