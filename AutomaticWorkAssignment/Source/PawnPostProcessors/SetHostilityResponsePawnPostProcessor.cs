using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetHostilityResponsePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public HostilityResponseMode ResponseMode;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.playerSettings != null)
            {
                pawn.playerSettings.hostilityResponse = ResponseMode;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ResponseMode, "responseMode");
        }
    }
}
