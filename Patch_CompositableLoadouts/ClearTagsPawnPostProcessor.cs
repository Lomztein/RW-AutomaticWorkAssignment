using Inventory;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class ClearTagsPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public LoadoutState State;
        private string _stateName;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                var tags = GetTagsSetByAWA(request);
                foreach (var tag in tags)
                {
                    var loadout = pawn.GetComp<LoadoutComponent>();
                    var loadoutElement = loadout.Loadout.AllElements.FirstOrDefault(el => el.Tag == tag && el.State == State);
                    if (loadoutElement != null)
                    {
                        loadout.RemoveTag(loadoutElement);
                    }
                }
            }
        }

        private IEnumerable<Tag> GetTagsSetByAWA(ResolveWorkRequest request)
        {
            foreach (var spec in request.WorkManager.WorkList)
            {
                foreach (var pp in spec.PostProcessors)
                {
                    foreach (Tag tag in GetTagsFrom(pp))
                    {
                        yield return tag;
                    }
                }
            }
        }

        private IEnumerable<Tag> GetTagsFrom(IPawnSetting setting)
        {
            if (setting is AddTagPawnPostProcessor addTagPP && addTagPP.Tag != null)
            {
                yield return addTagPP.Tag;
            }
            if (setting is ICompositePawnSetting composite)
            {
                foreach (var inner in composite.GetSettings())
                {
                    var tags = GetTagsFrom(inner);
                    foreach (var tag in tags)
                        yield return tag;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref State, "state");

            if (Scribe.mode == LoadSaveMode.Saving)
                _stateName = State?.name;

            Scribe_Values.Look(ref _stateName, "stateName");

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                if (_stateName != null & State == null)
                    State = LoadoutManager.States.FirstOrDefault(x => x.name == _stateName);
            }
        }
    }
}
