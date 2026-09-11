using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class RemoveMarkPawnPostProcessor : UsefulMarksMarkerPawnPostProcessor
    {
        public override void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            UsefulMarksCompatibility.RemoveMark(pawn, MarkerIndex);
        }
    }
}
