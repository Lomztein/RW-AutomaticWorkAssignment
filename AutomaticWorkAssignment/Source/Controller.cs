using AutomaticWorkAssignment.Source.UI.PawnFitness;
using AutomaticWorkAssignment.UI;
using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.UI.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Controller : Mod
    {
        public Controller(ModContentPack content) : base(content)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Initialize fitness UI handlers.
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<LearnRatePawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<SkillLevelPawnFitness>());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<StatPawnFitness, StatDef>(
                () => DefDatabase<StatDef>.AllDefs, x => x.label, x => x?.StatDef?.label ?? "Select stat", (c, s) => c.StatDef = s));

            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<InvertPawnFitness, PawnFitnessDef>());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<ConditionPawnFitness, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<CountPawnFitness, PawnConditionDef>("Add condition", false));
            PawnSettingUIHandlers.AddHandler(new ConstantPawnFitnessUIHandler());

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<AggregatePawnFitness, PawnFitnessDef>("Add function", false));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<AveragePawnFitness, PawnFitnessDef>("Add function", false));

            // Initialize condition UI handlers.
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<SlavePawnCondition>());
            PawnSettingUIHandlers.AddHandler(new SkillLevelPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new CompareFitnessPawnConditionUIHandler("Add operand", true));

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HediffPawnCondition, HediffDef>(
                () => DefDatabase<HediffDef>.AllDefs, x => x.label, x => x?.HediffDef?.label ?? "Select condition", (c, s) => c.HediffDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PassionPawnCondition, SkillDef>(
                () => DefDatabase<SkillDef>.AllDefs, x => x.label, x => x?.SkillDef?.label ?? "Select skill", (c, s) => c.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SpecificPawnCondition, Pawn>(
                () => WorkManager.Instance.GetAllEverAssignablePawns(), x => x.Name.ToString(), x => x?.Pawn?.Name.ToString() ?? "Select pawn", (c, s) => c.Pawn = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<TraitPawnCondition, TraitDef>(
                () => DefDatabase<TraitDef>.AllDefs, x => x.label ?? x.degreeDatas?.FirstOrDefault()?.label, x => x?.TraitDef?.label ?? x.TraitDef?.degreeDatas?.FirstOrDefault()?.label ?? "Select trait", (c, s) => c.TraitDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<WeaponClassPawnCondition, WeaponClassDef>(
                () => DefDatabase<WeaponClassDef>.AllDefs, x => x.label, x => x.WeaponClassDef?.label ?? "Select class", (c, s) => c.WeaponClassDef = s));

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<AnyPawnCondition, PawnConditionDef>("Add condition", false));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<AllPawnCondition, PawnConditionDef>("Add condition", false));
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<NotPawnCondition, PawnConditionDef>());

            // Biotech only
            if (ModLister.BiotechInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<DevelopmentalStagePawnCondition, DevelopmentalStage>(
                    () => ((DevelopmentalStage[])Enum.GetValues(typeof(DevelopmentalStage))).ToList(), x => x.ToString(), x => x?.DevelopmentalStage.ToString() ?? "Select stage", (c, s) => c.DevelopmentalStage = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<GenePawnCondition, GeneDef>(
                    () => DefDatabase<GeneDef>.AllDefs, x => x.label, x => x?.GeneDef?.label ?? "Select gene", (c, s) => c.GeneDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<XenotypePawnCondition, XenotypeDef>(
                    () => DefDatabase<XenotypeDef>.AllDefs, x => x.label, x => x?.XenotypeDef?.label ?? "Select xenotype", (c, s) => c.XenotypeDef = s));
            }


            // Initialize post processor UI handlers.
            PawnSettingUIHandlers.AddHandler(new SetTitlePawnPostProcessorUIHandler());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetApparelPolicyPawnPostProcessor, ApparelPolicy>(
                () => Current.Game.outfitDatabase.AllOutfits, (x) => x.label, (x) => x?.Policy?.label ?? "Select policy", (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetFoodPolicyPawnPostProcessor, FoodPolicy>(
                () => Current.Game.foodRestrictionDatabase.AllFoodRestrictions, (x) => x.label, (x) => x?.Policy?.label ?? "Select policy", (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetDrugPolicyPawnPostProcessor, DrugPolicy>(
                () => Current.Game.drugPolicyDatabase.AllPolicies, (x) => x.label, (x) => x?.Policy?.label ?? "Select policy", (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetReadingPolicyPawnPostProcessor, ReadingPolicy>(
                () => Current.Game.readingPolicyDatabase.AllReadingPolicies, (x) => x.label, (x) => x?.Policy?.label ?? "Select policy", (pp, po) => pp.Policy = po));

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetAllowedAreaPawnPostProcessor, Area>(
                () => new List<Area>() { null }.Concat(Current.Game.CurrentMap.areaManager.AllAreas).Where(x => x?.AssignableAsAllowed() ?? true), (x) => x?.Label ?? "Everywhere", (x) => x?.AllowedArea?.Label ?? "Everywhere", (pp, po) => pp.AllowedArea = po));
        }
    }
}
