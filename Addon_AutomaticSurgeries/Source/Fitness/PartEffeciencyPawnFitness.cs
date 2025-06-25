using AutomaticWorkAssignment;
using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class PartEffeciencyPawnFitness : PawnSetting, IPawnFitness
    {
        public BodyPartRecord BodyPartRecord
        {
            get { return _bodyPartIndex >= 0 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }
        private int _bodyPartIndex = -1;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => BodyPartRecord != null && pawn != null ? PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, BodyPartRecord) : 0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }
    }
}
