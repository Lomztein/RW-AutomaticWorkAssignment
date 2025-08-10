using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SkillLevelPawnCondition : PawnSetting, IPawnCondition
    {
        // TODO: Figure out if we can use PawnConditionDefs label/description somehow, instead.
        public SkillDef SkillDef;

        public int MinLevel = 0;
        public int MaxLevel = 20;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            int pawnLevel = SkillDef != null ? pawn.skills.GetSkill(SkillDef).Level : 0;
            return (pawnLevel >= MinLevel)
                && (pawnLevel <= MaxLevel);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref SkillDef, "skillDef");
            Scribe_Values.Look(ref MinLevel, "minLevel", 0);
            Scribe_Values.Look(ref MaxLevel, "maxLevel", 20);
        }
    }
}
