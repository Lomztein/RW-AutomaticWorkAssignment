using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class LearningSaturatedPawnCondition : PawnSetting, IPawnCondition
    {
        public SkillDef SkillDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => SkillDef != null && ((pawn?.skills.GetSkill(SkillDef).LearningSaturatedToday) ?? false);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }
    }
}
