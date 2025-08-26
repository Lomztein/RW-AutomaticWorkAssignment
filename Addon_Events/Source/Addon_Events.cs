using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.Source.UI.Modular;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
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
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<BleedingPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<DraftedPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<StopWorkPawnPostProcessor>());
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<DoingWorkPawnCondition, WorkTypeDef>(x => DefDatabase<WorkTypeDef>.AllDefs, x => x.labelShort, x => x.WorkTypeDef?.labelShort ?? "AWA.WorkTypeSelect".Translate(), (s, w) => s.WorkTypeDef = w));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<OnEventPawnPostProcessor>(
                new Picker<OnEventPawnPostProcessor, EventDef>(map => DefDatabase<EventDef>.AllDefs, x => x.LabelCap, x => x.EventDef?.LabelCap ?? "AWA.EventSelect".Translate(), (pp, po) => pp.EventDef = po),
                new Nested<OnEventPawnPostProcessor, PawnPostProcessorDef>(x => x.NestedPostProcessor, (pp, po) => pp.NestedPostProcessor = po as IPawnPostProcessor)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<DraftPawnPostProcessor>(new Toggle<DraftPawnPostProcessor>(x => x.Value, (x, v) => x.Value = v, (x => x.Value ? "AWA.Draft".Translate() : "AWA.Undraft".Translate()))));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<MoveToPawnPostProcessor>(
                new PositionTargeter<MoveToPawnPostProcessor>(x => x.MoveToPosition?.ToVector3(), (pp, pos) => pp.MoveToPosition = pos.ToIntVec3()),
                new Label<MoveToPawnPostProcessor>(x => "AWA.OnArrival".Translate()),
                new Nested<MoveToPawnPostProcessor, PawnPostProcessorDef>(x => x.OnArrived, (x, s) => x.OnArrived = (IPawnPostProcessor)s)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<DoAfterDelayPawnPostProcessor>(
                new Splitter<DoAfterDelayPawnPostProcessor>(
                    new TextFieldNumeric<float, DoAfterDelayPawnPostProcessor>(x => x.DelayHours, (x, v) => x.DelayHours = v),
                    new Label<DoAfterDelayPawnPostProcessor>(x => "AWA.Hours".Translate(), TextAnchor.MiddleLeft)),
                new Nested<DoAfterDelayPawnPostProcessor, PawnPostProcessorDef>(x => x.Action, (x, p) => x.Action = (IPawnPostProcessor)p)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<DoRepeatPawnPostProcessor>(
                new Splitter<DoRepeatPawnPostProcessor>(
                    new TextFieldNumeric<float, DoRepeatPawnPostProcessor>(x => x.DelayHours, (x, v) => x.DelayHours = v),
                    new Label<DoRepeatPawnPostProcessor>(x => "AWA.Hours".Translate(), TextAnchor.MiddleLeft)),
                new Nested<DoRepeatPawnPostProcessor, PawnPostProcessorDef>(x => x.Action, (x, p) => x.Action = (IPawnPostProcessor)p)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<UseAbilityPawnPostProcessor>(new Picker<UseAbilityPawnPostProcessor, AbilityDef>(x => DefDatabase<AbilityDef>.AllDefs, x => x.LabelCap, x => x.AbilityDef?.LabelCap ?? "AWA.AbilitySelect".Translate(), (pp, ad) => pp.AbilityDef = ad)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<UseAbilityOnAllyPawnPostProcessor>(
                new Picker<UseAbilityOnAllyPawnPostProcessor, AbilityDef>(x => DefDatabase<AbilityDef>.AllDefs, x => x.LabelCap, x => x.AbilityDef?.LabelCap ?? "AWA.AbilitySelect".Translate(), (pp, ad) => pp.AbilityDef = ad),
                new Composite<UseAbilityOnAllyPawnPostProcessor, IPawnFitness, PawnFitnessDef>(x => x.Fitness, "AWA.FunctionAdd".Translate()),
                new Composite<UseAbilityOnAllyPawnPostProcessor, IPawnCondition, PawnConditionDef>(x => x.Conditions, "AWA.ConditionAdd".Translate())));


            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<UseAbilityOnEnemyPawnPostProcessor>(
                new Picker<UseAbilityOnEnemyPawnPostProcessor, AbilityDef>(x => DefDatabase<AbilityDef>.AllDefs, x => x.LabelCap, x => x.AbilityDef?.LabelCap ?? "AWA.AbilitySelect".Translate(), (pp, ad) => pp.AbilityDef = ad),
                new Composite<UseAbilityOnEnemyPawnPostProcessor, IPawnFitness, PawnFitnessDef>(x => x.Fitness, "AWA.FunctionAdd".Translate()),
                new Composite<UseAbilityOnEnemyPawnPostProcessor, IPawnCondition, PawnConditionDef>(x => x.Conditions, "AWA.ConditionAdd".Translate())));
        }
    }
}
