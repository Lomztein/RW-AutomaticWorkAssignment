using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PartAnyHediffPawnCondition : PawnSetting, IPawnCondition
    {
        public BodyPartRecord BodyPartRecord
        {
            get { return _bodyPartIndex >= 0 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }
        private int _bodyPartIndex = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn?.health?.hediffSet?.hediffs == null || BodyPartRecord == null)
                return false;
                
            return pawn.health.hediffSet.hediffs.Any(x => 
                x.Part != null && 
                x.Part.LabelCap == BodyPartRecord.LabelCap);
        }
    }
}
