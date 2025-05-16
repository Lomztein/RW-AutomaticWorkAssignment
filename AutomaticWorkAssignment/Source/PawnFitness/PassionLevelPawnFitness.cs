using Lomzie.AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class PassionLevelPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            float sum = 0;
            int amount = 0;
            foreach (WorkTypeDef workDef in specification.Priorities.OrderedPriorities)
            {
                foreach (SkillDef skillDef in workDef.relevantSkills)
                {
                    SkillRecord skillRecord = pawn.skills.GetSkill(skillDef);
                    if (skillRecord.LearnRateFactor(true) >= 0.99f)
                    {
                        sum += (int)skillRecord.passion;
                        amount++;
                    }
                }
            }

            if (amount > 0)
                return sum / amount;
            else return 0;
        }
    }
}
