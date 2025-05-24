using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class LearnRatePawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            var workTypeDefs = specification.Priorities.OrderedPriorities;
            float sum = 0;
            int total = 0;
            foreach (var workTypeDef in workTypeDefs)
            {
                var relavantSkills = workTypeDef.relevantSkills;
                foreach (var skill in  relavantSkills)
                {
                    sum += pawn.skills.GetSkill(skill).LearnRateFactor();
                    total++;
                }
            }
            if (total == 0)
                return 0;
            return sum / total;
        }
    }
}
