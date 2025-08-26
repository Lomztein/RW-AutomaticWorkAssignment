using Inventory;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class SatisfyLoadoutPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            var loadoutComp = pawn.GetComp<LoadoutComponent>();
            if (loadoutComp != null && loadoutComp.Loadout != null)
                loadoutComp.Loadout.RequiresUpdate();
        }
    }
}
