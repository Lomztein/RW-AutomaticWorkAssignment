using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
