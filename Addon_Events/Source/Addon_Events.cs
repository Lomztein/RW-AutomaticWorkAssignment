using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
using System;
using Verse;

namespace Lomztein.AutomaticWorkAssignments
{
    public class Addon_Events : Mod
    {
        public Addon_Events(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(InitializePawnSettingUIHandlers);
        }

        private void InitializePawnSettingUIHandlers()
        {
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<OnEventPawnPostProcessor>(
                new Picker<OnEventPawnPostProcessor, EventDef>(map => DefDatabase<EventDef>.AllDefs, x => x.LabelCap, x => x.EventDef?.LabelCap ?? "AWA.EventSelect".Translate(), (pp, po) => pp.EventDef = po),
                new Nested<OnEventPawnPostProcessor, PawnPostProcessorDef>(x => x.NestedPostProcessor, (pp, po) => pp.NestedPostProcessor = po as IPawnPostProcessor)));

        }
    }
}
