using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class TraitPawnCondition : PawnSetting, IPawnCondition
    {
        public TraitDef TraitDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref TraitDef, "traitDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.story?.traits?.HasTrait(TraitDef) ?? false;
    }
}
