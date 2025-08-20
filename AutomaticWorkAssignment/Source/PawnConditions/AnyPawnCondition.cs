using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AnyPawnCondition : CompositePawnSetting<IPawnCondition>, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => InnerSettings.Any(x => x is IPawnCondition condition && condition.IsValid(pawn, specification, request));
    }
}
