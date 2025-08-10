using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class StockpilePawnCondition : PawnSetting, IPawnCondition
    {
        public ThingFilter ThingFilter;

        public StockpilePawnCondition()
        {
            ThingFilter = new ThingFilter(DefDatabase<ThingCategoryDef>.GetNamed("Root"));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn.Map.listerThings.ThingsMatchingFilter(ThingFilter).Sum(x => x.stackCount) > 0;
        }
    }
}
