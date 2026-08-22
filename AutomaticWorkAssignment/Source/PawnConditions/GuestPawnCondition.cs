using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class GuestPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return Utils.IsGuest(pawn, request.Map);
        }
    }
}
