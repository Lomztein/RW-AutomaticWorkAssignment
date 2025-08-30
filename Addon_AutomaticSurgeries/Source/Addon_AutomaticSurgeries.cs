using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Addon_AutomaticSurgeries : Mod
    {
        public Addon_AutomaticSurgeries(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(InitializePawnSettingUIHandlers);
        }

        private void InitializePawnSettingUIHandlers()
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PartEffeciencyPawnFitness, BodyPartRecord>(
                (m) => BodyDefOf.Human.AllParts, x => x.LabelCap, x => x.BodyPartRecord?.LabelCap ?? "AWA.BodyPartSelect".Translate(), (c, s) => c.BodyPartRecord = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PartMissingPawnCondition, BodyPartRecord>(
                (m) => BodyDefOf.Human.AllParts, x => x.LabelCap, x => x.BodyPartRecord?.LabelCap ?? "AWA.BodyPartSelect".Translate(), (c, s) => c.BodyPartRecord = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PartAnyHediffPawnCondition, BodyPartRecord>(
                (m) => BodyDefOf.Human.AllParts, x => x.LabelCap, x => x.BodyPartRecord?.LabelCap ?? "AWA.BodyPartSelect".Translate(), (c, s) => c.BodyPartRecord = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<ImplantXenogermPawnPostProcessor, string>(
                (m) => ImplantXenogermPawnPostProcessor.GetValidXenogermLabels(), x => x, x => x.XenogermLabel ?? "AWA.XenogermSelect".Translate(), (c, s) => c.XenogermLabel = s));

            PawnSettingUIHandlers.AddHandler(new PartHediffPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new AddBillPawnPostProcessorUIHandler());
        }
    }
}
