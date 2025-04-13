using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class LearnRatePawnFitness : IPawnFitness
    {
        public string Label => "Learning rate";
        public string Description => "Pawn learning rate for related skills.";

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

        public void ExposeData()
        {
        }
    }
}
