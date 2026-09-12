using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class RemoveMarkPawnPostProcessor : MarkPawnPostProcessorBase
    {
        public override void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            UsefulMarksCompatibility.RemoveMark(pawn, MarkerIndex);
        }

        public override bool IsConfigured()
        {
            return MarkerIndex != -1 || UsefulMarksCompatibility.IsValidMarkerIndex(MarkerIndex);
        }
    }
}
