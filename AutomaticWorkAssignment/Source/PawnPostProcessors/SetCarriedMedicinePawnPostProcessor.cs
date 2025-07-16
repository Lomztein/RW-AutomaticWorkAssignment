using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
