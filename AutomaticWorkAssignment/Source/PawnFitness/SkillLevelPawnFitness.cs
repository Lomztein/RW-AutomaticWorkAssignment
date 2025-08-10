using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class SkillLevelPawnFitness : PawnSetting, IPawnFitness
    {
        public SkillDef SkillDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (SkillDef == null)
            {
                var workTypeDefs = specification.Priorities.OrderedPriorities;
                float sum = 0;
                int total = 0;
                foreach (var workTypeDef in workTypeDefs)
                {
                    var relavantSkills = workTypeDef.relevantSkills;
                    foreach (var skill in relavantSkills)
                    {
                        sum += pawn.skills.GetSkill(skill).Level;
                        total++;
                    }
                }
                if (total == 0)
                    return 0;
                return sum / total;
            }
            else
            {
                return pawn?.skills.GetSkill(SkillDef).Level ?? 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }
    }
}
