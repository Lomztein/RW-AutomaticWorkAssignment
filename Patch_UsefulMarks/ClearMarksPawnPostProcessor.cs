using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class ClearMarksPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            UsefulMarksCompatibility.ClearMarks(pawn);
        }
    }
}
