using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class HediffPawnCondition : PawnSetting, IPawnCondition
    {
        public HediffDef HediffDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref HediffDef, "hediffDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.health.hediffSet?.HasHediff(HediffDef) ?? false;
    }
}
