using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AnyPawnPawnCondition : NestedPawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSetting != null && InnerSetting is IPawnCondition condition)
            {
                foreach (var pawn1 in request.Pawns)
                {
                    if (condition.IsValid(pawn1, specification, request))
                        return true;
                }
            }
            return false;
        }
    }
}
