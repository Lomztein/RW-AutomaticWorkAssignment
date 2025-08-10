using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
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
