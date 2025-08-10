using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class GuestPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (request.Map.ParentFaction != null)
            {
                return pawn.HomeFaction != request.Map.ParentFaction;
            }
            return false;
        }
    }
}
