using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetAllowedAreaPawnPostProcessor : IPawnPostProcessor
    {
        public string Label => "Set allowed area";
        public string Description => "Set the allowed area of the pawn.";

        public Area AllowedArea;

        public void ExposeData()
        {
            Scribe_References.Look(ref AllowedArea, "area");
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn.Map?.IsPlayerHome ?? false)
            {
                pawn.playerSettings.AreaRestrictionInPawnCurrentMap = AllowedArea;
            }
        }
    }
}
