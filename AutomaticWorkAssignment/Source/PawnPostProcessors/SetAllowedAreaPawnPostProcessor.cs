using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetAllowedAreaPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Area AllowedArea;

        public override void ExposeData()
        {
            base.ExposeData();
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
