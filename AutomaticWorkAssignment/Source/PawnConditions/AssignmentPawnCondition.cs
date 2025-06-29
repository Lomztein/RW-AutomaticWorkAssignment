using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AssignmentPawnCondition : PawnSetting, IPawnCondition
    {
        public WorkSpecification WorkSpec;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                return request.WorkManager.GetAssignmentTo(pawn, WorkSpec) != null;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref WorkSpec, "workSpec");
        }
    }
}
