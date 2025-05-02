using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AnyPawnCondition : CompositePawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => InnerSettings.Any(x => x is IPawnCondition condition && condition.IsValid(pawn, specification, request));
    }
}
