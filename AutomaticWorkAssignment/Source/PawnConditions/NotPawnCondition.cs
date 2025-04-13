using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class NotPawnCondition : NestedPawnSetting, IPawnCondition
    {
        public override string Label => "Not";
        public override string Description => "Inverts the nested condition.";

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
