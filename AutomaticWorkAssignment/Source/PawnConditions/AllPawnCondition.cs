using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AllPawnCondition : CompositePawnSetting, IPawnCondition
    {
        public override string Label => "All";
        public override string Description => "Check if all of the nested conditions are true.";

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => InnerSettings.All(x => x is IPawnCondition condition && condition.IsValid(pawn, specification, request));
    }
}
