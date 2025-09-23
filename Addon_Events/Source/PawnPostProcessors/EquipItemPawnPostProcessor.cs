using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class EquipItemPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public ThingFilter ThingFilter = new ThingFilter(ThingCategoryDefOf.Weapons);

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && ThingFilter != null)
            {
                Thing thing = pawn.inventory.innerContainer.FirstOrDefault(x => ThingFilter.Allows(x));
                if (thing != null && thing is ThingWithComps thingWithComps)
                {
                    if (thing.TryGetComp(out CompEquippable equippable))
                    {
                        pawn.equipment.MakeRoomFor(thingWithComps, out ThingWithComps dropped);
                        if (dropped != null)
                            pawn.equipment.TryTransferEquipmentToContainer(dropped, pawn.inventory.GetDirectlyHeldThings());
                    }
                    pawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(thing);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
        }
    }
}
