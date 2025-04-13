using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class SkillLevelPawnFitness : IPawnFitness
    {
        public string Label => "Skill level";
        public string Description => "Uses pawn skill level relevant skills for fitness.";

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            var workTypeDefs = specification.Priorities.OrderedPriorities;
            float sum = 0;
            foreach (var workTypeDef in workTypeDefs)
            {
                var relavantSkills = workTypeDef.relevantSkills;
                sum += relavantSkills.Sum(y => pawn.skills.GetSkill(y).Level);
            }
            return sum;
        }

        public void ExposeData()
        {
        }
    }
}
