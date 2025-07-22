using Inventory;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class AddTagPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Tag Tag;
        public LoadoutState State;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Tag != null)
            {
                var loadoutComp = pawn.GetComp<LoadoutComponent>();
                var element = loadoutComp.Loadout.AllElements.FirstOrDefault(x => x.Tag == Tag && x.State == State);
                if (loadoutComp != null && element == null)
                {
                    loadoutComp.AddElement(new LoadoutElement(Tag, State));
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Tag, "Tag");
            Scribe_References.Look(ref State, "state");
        }
    }
}
