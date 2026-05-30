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
            Scribe_Values.Look(ref RequireAll, "requireAll", true);
            Scribe_Collections.Look(ref RequiredCapabilities, "requiredCapabilities");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null) return false;
            if (RequireAll)
            {
                return RequiredCapabilities.All(x => !Utils.WorkTypeIsDisabled(pawn, x));
            }
            else
            {
                return RequiredCapabilities.Any(x => !Utils.WorkTypeIsDisabled(pawn, x));
            }
        }
    }
}
