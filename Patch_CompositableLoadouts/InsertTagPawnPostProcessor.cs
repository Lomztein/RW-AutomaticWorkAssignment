using Inventory;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class InsertTagPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Tag Tag;
        public LoadoutState State;
        public Tag RelativeTo;
        public bool InsertAfter = false;

        private string _tagName;
        private string _relativeToName;
        private string _stateName;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Tag != null)
            {
                var loadoutComp = pawn.GetComp<LoadoutComponent>();
                var element = loadoutComp.Loadout.AllElements.FirstOrDefault(x => x.Tag == Tag && x.State == State);
                if (loadoutComp != null && element == null)
                {
                    if (RelativeTo != null)
                    {
                        var index = loadoutComp.Loadout.AllElements.FirstIndexOf(x => x.Tag == RelativeTo && x.State == State);
                        if (index != -1)
                        {
                            if (InsertAfter)
                                index++;
                            loadoutComp.InsertElement(new LoadoutElement(Tag, State), index);
                            return;
                        }
                    }
                    if (InsertAfter)
                    {
                        loadoutComp.InsertElement(new LoadoutElement(Tag, State), loadoutComp.Loadout.AllElements.Count());
                    }
                    else
                    {
                        loadoutComp.InsertElement(new LoadoutElement(Tag, State), 0);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Tag, "Tag");
            Scribe_References.Look(ref RelativeTo, "RelativeTo");
            Scribe_References.Look(ref State, "state");
            Scribe_Values.Look(ref InsertAfter, "InsertAfter", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _tagName = Tag?.name;
                _relativeToName = RelativeTo?.name;
                _stateName = State?.name;
            }

            Scribe_Values.Look(ref _tagName, "tagName");
            Scribe_Values.Look(ref _relativeToName, "relativeToName");
            Scribe_Values.Look(ref _stateName, "stateName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_tagName != null && Tag == null)
                    Tag = LoadoutManager.Tags.FirstOrDefault(x => x.name == _tagName);
                if (_relativeToName != null && RelativeTo == null)
                    RelativeTo = LoadoutManager.Tags.FirstOrDefault(x => x.name == _relativeToName);
                if (_stateName != null & State == null)
                    State = LoadoutManager.States.FirstOrDefault(x => x.name == _stateName);
            }
        }
    }
}
