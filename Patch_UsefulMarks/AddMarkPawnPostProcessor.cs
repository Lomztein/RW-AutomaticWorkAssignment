using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class AddMarkPawnPostProcessor : UsefulMarksMarkerPawnPostProcessor
    {
        public override void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            UsefulMarksCompatibility.AddMark(pawn, MarkerIndex);
        }
    }
}
