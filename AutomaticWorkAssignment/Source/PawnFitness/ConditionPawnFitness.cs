using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ConditionPawnFitness : NestedPawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSetting != null)
            {
                IPawnCondition condition = InnerSetting as IPawnCondition;
                return condition.IsValid(pawn, specification, request) ? 1 : 0;
            }
            return 0;
        }
    }
}
