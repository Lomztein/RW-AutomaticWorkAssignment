using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PartMissingPawnCondition : PawnSetting, IPawnCondition
    {
        public BodyPartRecord BodyPartRecord
        {
            get { return _bodyPartIndex > 0 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }
        private int _bodyPartIndex = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.health.hediffSet?.PartIsMissing(BodyPartRecord) ?? false;
    }
}
