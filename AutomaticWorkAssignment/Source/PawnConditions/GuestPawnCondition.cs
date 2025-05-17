using AutomaticWorkAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
