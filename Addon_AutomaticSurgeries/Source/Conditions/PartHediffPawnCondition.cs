using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PartHediffPawnCondition : PawnSetting, IPawnCondition
    {
        public HediffDef HediffDef;
        public BodyPartRecord HediffPart
        {
            get { return _bodyPartIndex != -1 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }
        private int _bodyPartIndex = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref HediffDef, "hediffDef");
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn?.health?.hediffSet?.hediffs == null || HediffDef == null || HediffPart == null)
                return false;
                
            return pawn.health.hediffSet.hediffs.Any(x => 
                x.def == HediffDef && 
                x.Part != null && 
                x.Part.LabelCap == HediffPart.LabelCap);
        }
    }
}
