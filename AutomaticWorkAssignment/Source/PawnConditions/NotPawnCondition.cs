using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class NotPawnCondition : NestedPawnCondition
    {
        public override string Label => "Not";
        public override string Description => "Inverts the nested condition.";

        public override bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerCondition != null)
            {
                return InnerCondition.IsValid(pawn, specification, request);
            }
            return true;
        }
    }
}
