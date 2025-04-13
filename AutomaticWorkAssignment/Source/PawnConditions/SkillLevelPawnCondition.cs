using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SkillLevelPawnCondition : IPawnCondition
    {
        // TODO: Figure out if we can use PawnConditionDefs label/description somehow, instead.
        public string Label => "Skill level";
        public string Description => "Only assign pawns with the given skill between set levels.";

        public SkillDef SkillDef;

        public int MinLevel = 0;
        public int MaxLevel = 20;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            int pawnLevel = SkillDef != null ? pawn.skills.GetSkill(SkillDef).Level : 0;
            return (pawnLevel >= MinLevel)
                && (pawnLevel <= MaxLevel);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SkillDef, "skillDef");
            Scribe_Values.Look(ref MinLevel, "minLevel", 0);
            Scribe_Values.Look(ref MaxLevel, "maxLevel", 20);
        }
    }
}
