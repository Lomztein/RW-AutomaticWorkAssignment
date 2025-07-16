using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class EquipmentPawnCondition : PawnSetting, IPawnCondition
    {
        public ThingFilter ThingFilter;

        public EquipmentPawnCondition()
        {
            ThingFilter = new ThingFilter(DefDatabase<ThingCategoryDef>.GetNamed("Apperal"));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn?.equipment.AllEquipmentListForReading.Any(x => ThingFilter.Allows(x)) ?? false;
        }
    }
}
