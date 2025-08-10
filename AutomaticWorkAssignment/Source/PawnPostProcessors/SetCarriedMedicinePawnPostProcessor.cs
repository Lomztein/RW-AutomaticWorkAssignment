using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetCarriedMedicinePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public int Count;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            pawn?.inventoryStock.SetCountForGroup(InventoryStockGroupDefOf.Medicine, Count);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Count, "count");
        }
    }
}
