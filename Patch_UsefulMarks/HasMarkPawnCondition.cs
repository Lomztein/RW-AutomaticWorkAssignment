using AutomaticWorkAssignment;
using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class HasMarkPawnCondition : PawnSetting, IPawnCondition
    {
        public int MarkerIndex = -1;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return UsefulMarksCompatibility.HasMark(pawn, MarkerIndex);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MarkerIndex, "markerIndex", -1);
        }
    }
}
