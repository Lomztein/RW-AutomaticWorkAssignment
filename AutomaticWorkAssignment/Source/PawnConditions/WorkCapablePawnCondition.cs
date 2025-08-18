using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class WorkCapablePawnCondition : PawnSetting, IPawnCondition
    {
        public bool RequireAll;
        public List<WorkTypeDef> RequiredCapabilities = new List<WorkTypeDef>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref RequiredCapabilities, "requiredCapabilities");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null) return false;
            if (RequireAll)
            {
                return !pawn.OneOfWorkTypesIsDisabled(RequiredCapabilities);
            }
            else
            {
                return RequiredCapabilities.Any(x => !pawn.WorkTypeIsDisabled(x));
            }
        }
    }
}
