using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class WeaponPawnCondition : PawnSetting, IPawnCondition
    {
        public ThingFilter ThingFilter;

        public WeaponPawnCondition()
        {
            ThingFilter = new ThingFilter(DefDatabase<ThingCategoryDef>.GetNamed("Weapons"));
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
