using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public abstract class UsefulMarksMarkerPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public int MarkerIndex = -1;

        public abstract void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MarkerIndex, "markerIndex", -1);
        }
    }
}
