using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class WeaponClassPawnCondition : PawnSetting, IPawnCondition
    {
        public WeaponClassDef WeaponClassDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref WeaponClassDef, "weaponClassDef");
        }

        public override bool IsConfigured() => WeaponClassDef != null;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (WeaponClassDef != null)
            {
                return pawn.equipment.Primary?.def.weaponClasses?.Contains(WeaponClassDef) ?? false;
            }
            return false;
        }
    }
}
