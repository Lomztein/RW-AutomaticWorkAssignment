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
    public class ApparelPawnCondition : PawnSetting, IPawnCondition
    {
        public ThingFilter ThingFilter;

        public ApparelPawnCondition()
        {
            ThingFilter = new ThingFilter(DefDatabase<ThingCategoryDef>.GetNamed("Apparel"));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn?.apparel.WornApparel.Any(x => ThingFilter.Allows(x)) ?? false;
        }
    }
}
