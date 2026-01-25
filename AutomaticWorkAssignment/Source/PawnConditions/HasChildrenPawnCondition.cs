using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class HasChildrenPawnCondition : CompositePawnSetting<IPawnCondition>, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null || pawn.relations == null)
                return false;

            if (InnerSettings == null)
                return pawn.relations.ChildrenCount != 0;

            foreach (var child in pawn.relations.Children)
            {
                if (InnerSettings.All(x => x.IsValid(child, specification, request)))
                    return true;
            }
            return false;
        }
    }
}
