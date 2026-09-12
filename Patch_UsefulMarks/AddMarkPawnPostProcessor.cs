using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class AddMarkPawnPostProcessor : MarkPawnPostProcessorBase
    {
        public override void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            UsefulMarksCompatibility.AddMark(pawn, MarkerIndex);
        }

        public override bool IsConfigured()
        {
            return MarkerIndex != -1 || UsefulMarksCompatibility.IsValidMarkerIndex(MarkerIndex);
        }
    }
}
