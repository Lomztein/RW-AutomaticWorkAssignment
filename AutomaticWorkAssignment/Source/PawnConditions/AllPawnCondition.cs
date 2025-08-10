using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AllPawnCondition : CompositePawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => InnerSettings.All(x => x is IPawnCondition condition && condition.IsValid(pawn, specification, request));
    }
}
