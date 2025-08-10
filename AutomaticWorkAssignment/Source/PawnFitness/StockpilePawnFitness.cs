using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class StockpilePawnFitness : PawnSetting, IPawnFitness
    {
        public ThingFilter ThingFilter;

        public StockpilePawnFitness()
        {
            ThingFilter = new ThingFilter(DefDatabase<ThingCategoryDef>.GetNamed("Root"));
        }

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn.Map.listerThings.ThingsMatchingFilter(ThingFilter).Sum(x => x.stackCount);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
        }
    }
}
