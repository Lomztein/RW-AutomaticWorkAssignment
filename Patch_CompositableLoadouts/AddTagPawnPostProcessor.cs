using Inventory;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class AddTagPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Tag Tag;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                var loadoutComp = pawn.GetComp<LoadoutComponent>();
                if (loadoutComp != null && !loadoutComp.Loadout.AllTags.Contains(Tag))
                {
                    loadoutComp.AddTag(Tag);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Tag, "Tag");
        }
    }
}
