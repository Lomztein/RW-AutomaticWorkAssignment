using Inventory;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class ClearTagsPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                var tags = GetTagsSetByAWA(request);
                foreach (var tag in tags )
                {
                    var loadout = pawn.GetComp<LoadoutComponent>();
                    var loadoutElement = loadout.Loadout.AllElements.FirstOrDefault(el => el.Tag == tag);
                    if (loadoutElement != null)
                    {
                        loadout.RemoveTag(loadoutElement);
                    }
                }
            }
        }

        private IEnumerable<Tag> GetTagsSetByAWA (ResolveWorkRequest request)
        {
            foreach (var spec in request.WorkManager.WorkList)
            {
                foreach (var pp in spec.PostProcessors)
                {
                    if (pp is AddTagPawnPostProcessor addTagPP && addTagPP.Tag != null)
                    {
                        yield return addTagPP.Tag;
                    }
                }
            }
        }
    }
}