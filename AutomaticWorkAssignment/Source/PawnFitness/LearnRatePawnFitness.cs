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
            foreach (var workTypeDef in workTypeDefs)
            {
                var relavantSkills = workTypeDef.relevantSkills;
                sum += relavantSkills.Sum(y => pawn.skills.GetSkill(y).LearnRateFactor());
            }
            return sum;
        }
    }
}
