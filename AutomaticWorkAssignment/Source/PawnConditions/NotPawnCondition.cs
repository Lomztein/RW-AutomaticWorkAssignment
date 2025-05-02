using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class NotPawnCondition : NestedPawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSetting != null && InnerSetting is IPawnCondition condition)
            {
                return !condition.IsValid(pawn, specification, request);
            }
            return true;
        }
    }
}
