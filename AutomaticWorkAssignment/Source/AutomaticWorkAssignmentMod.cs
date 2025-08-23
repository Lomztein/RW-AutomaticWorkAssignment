using AutomaticWorkAssignment.UI;
using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
using Lomzie.AutomaticWorkAssignment.UI.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor;
using Lomzie.AutomaticWorkAssignment.UI.Windows;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class AutomaticWorkAssignmentMod : Mod
    {
        public static AutomaticWorkAssignmentSettings Settings;

        public AutomaticWorkAssignmentMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<AutomaticWorkAssignmentSettings>();
            LongEventHandler.ExecuteWhenFinished(InitializePawnSettingUIHandlers);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoWindow(inRect);
        }

        public override string SettingsCategory()
            => "Automatic Work Assignment";

        private void InitializePawnSettingUIHandlers()
        {
            // Initialize fitness UI handlers.
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<PassionCountPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<CommitmentPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<OrderingPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<AgePawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<HealthPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<StockpilePawnFitness>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<StatPawnFitness, StatDef>(
                (m) => DefDatabase<StatDef>.AllDefs.Where(x => x.showOnPawns), x => x.LabelCap, x => x?.StatDef?.LabelCap ?? "AWA.StatSelect".Translate(), (c, s) => c.StatDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<LearnRatePawnFitness, SkillDef>(
                (m) => new SkillDef[] { null }.Concat(DefDatabase<SkillDef>.AllDefs), x => x?.LabelCap ?? "AWA.Auto".Translate(), x => x.SkillDef?.LabelCap ?? "AWA.Auto".Translate(), (f, s) => f.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SkillLevelPawnFitness, SkillDef>(
                (m) => new SkillDef[] { null }.Concat(DefDatabase<SkillDef>.AllDefs), x => x?.LabelCap ?? "AWA.Auto".Translate(), x => x.SkillDef?.LabelCap ?? "AWA.Auto".Translate(), (f, s) => f.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PassionLevelPawnFitness, SkillDef>(
                (m) => new SkillDef[] { null }.Concat(DefDatabase<SkillDef>.AllDefs), x => x?.LabelCap ?? "AWA.Auto".Translate(), x => x.SkillDef?.LabelCap ?? "AWA.Auto".Translate(), (f, s) => f.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<NeedPawnFitness, NeedDef>(
                (m) => DefDatabase<NeedDef>.AllDefs, x => x.LabelCap, x => x.NeedDef?.LabelCap ?? "AWA.NeedSelect".Translate(), (f, s) => f.NeedDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<CapacityPawnFitness, PawnCapacityDef>(
                (m) => DefDatabase<PawnCapacityDef>.AllDefs, x => x.LabelCap, x => x?.CapacityDef?.LabelCap ?? "AWA.CapacitySelect".Translate(), (c, s) => c.CapacityDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<ImmunityMarginPawnFitness, HediffDef>(
                (m) => new HediffDef[] { null }.Concat(ImmunityMarginPawnFitness.GetApplicableHediffDefs()), x => x?.LabelCap ?? "AWA.WorstCase".Translate(), x => x?.Hediff?.LabelCap ?? "AWA.WorstCase".Translate(), (c, s) => c.Hediff = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<TendProgressPawnFitness, HediffDef>(
                (m) => new HediffDef[] { null }.Concat(TendProgressPawnFitness.GetApplicableHediffDefs()), x => x?.LabelCap ?? "AWA.WorstCase".Translate(), x => x?.Hediff?.LabelCap ?? "AWA.WorstCase".Translate(), (c, s) => c.Hediff = s));

            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<InvertPawnFitness, PawnFitnessDef>());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<ConditionPawnFitness, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, CountPawnFitness, PawnConditionDef>("AWA.ConditionAdd".Translate(), false));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, CountPawnsPawnFitness, PawnConditionDef>("AWA.ConditionAdd".Translate(), false));
            PawnSettingUIHandlers.AddHandler(new ConstantPawnFitnessUIHandler());
            PawnSettingUIHandlers.AddHandler(new FormulaPawnFitnessUIHandler());

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, AggregatePawnFitness, PawnFitnessDef>("AWA.FunctionAdd".Translate(), false));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, AveragePawnFitness, PawnFitnessDef>("AWA.FunctionAdd".Translate(), false));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ModulusPawnFitness>(
                new Nested<ModulusPawnFitness, PawnFitnessDef>(x => x.LeftHandSide, (x, setting) => x.LeftHandSide = setting as IPawnFitness),
                new Label<ModulusPawnFitness>(x => "%"),
                new Nested<ModulusPawnFitness, PawnFitnessDef>(x => x.RightHandSide, (x, setting) => x.RightHandSide = setting as IPawnFitness)
            ));

            // Initialize condition UI handlers.
            PawnSettingUIHandlers.AddHandler(new CommitmentLimitPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<SlavePawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<GuestPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<MutantPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new SkillLevelPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new FitnessInRangePawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new CompareFitnessPawnConditionUIHandler());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HediffPawnCondition, HediffDef>(
                (m) => DefDatabase<HediffDef>.AllDefs, x => x.LabelCap, x => x?.HediffDef?.LabelCap ?? "AWA.ConditionSelect".Translate(), (c, s) => c.HediffDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PassionPawnCondition, SkillDef>(
                (m) => DefDatabase<SkillDef>.AllDefs, x => x.LabelCap, x => x?.SkillDef?.LabelCap ?? "AWA.SkillSelect".Translate(), (c, s) => c.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<LearningSaturatedPawnCondition, SkillDef>(
                (m) => DefDatabase<SkillDef>.AllDefs, x => x.LabelCap, x => x?.SkillDef?.LabelCap ?? "AWA.SkillSelect".Translate(), (c, s) => c.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SpecificPawnCondition, Pawn>(
                (m) => MapWorkManager.GetManager(m).GetAllPawns(), x => x.Name.ToString(), x => x?.Pawn?.Name.ToString() ?? "AWA.PawnSelect".Translate(), (c, s) => c.Pawn = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<TraitPawnCondition, TraitDef>(
                (m) => DefDatabase<TraitDef>.AllDefs, x => x.degreeDatas?.FirstOrDefault()?.label ?? x.label, x => x?.TraitDef?.label ?? x.TraitDef?.degreeDatas?.FirstOrDefault()?.label ?? "AWA.TraitSelect".Translate(), (c, s) => c.TraitDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<WeaponClassPawnCondition, WeaponClassDef>(
                (m) => DefDatabase<WeaponClassDef>.AllDefs, x => x.LabelCap, x => x.WeaponClassDef?.LabelCap ?? "AWA.ClassSelect".Translate(), (c, s) => c.WeaponClassDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<AssignmentPawnCondition, WorkSpecification>(
                (m) => MapWorkManager.GetManager(m).WorkList.Where(x => MapWorkManager.GetManager(m).WorkList.IndexOf(x) < MapWorkManager.GetManager(m).WorkList.IndexOf(WorkManagerWindow.CurrentRenderSpec)), x => x.Name, x => x.WorkSpec?.Name ?? "AWA.WorkSpecSelect".Translate(), (c, s) => c.WorkSpec = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PlanetLayerPawnCondition, PlanetLayerDef>(
                (m) => DefDatabase<PlanetLayerDef>.AllDefs, x => x.LabelCap, x => x.LayerDef?.LabelCap ?? "AWA.PlanetLayerSelect".Translate(), (c, s) => c.LayerDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<GenderPawnCondition, Gender>(
                (m) => (Gender[])Enum.GetValues(typeof(Gender)), (x) => GenderUtility.GetLabel(x), (x) => GenderUtility.GetLabel(x.Gender), (pp, po) => pp.Gender = po));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<WorkCapablePawnCondition>(new Toggle<WorkCapablePawnCondition>(x => x.RequireAll, (x, y) => x.RequireAll = y, x => "AWA.RequireAll".Translate()), new Lister<WorkCapablePawnCondition, WorkTypeDef, WorkTypeDef>((rect, e) => Widgets.Label(rect, e.labelShort), AutomaticWorkAssignmentSettings.UILabelSizeBase, x => DefDatabase<WorkTypeDef>.AllDefs, x => x.labelShort, x => x.RequiredCapabilities, (x, y) => y, null)));

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, AnyPawnCondition, PawnConditionDef>("AWA.ConditionSelect".Translate(), false));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, AllPawnCondition, PawnConditionDef>("AWA.ConditionSelect".Translate(), false));
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<NotPawnCondition, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<AnyPawnPawnCondition, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<StockpilePawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<ApparelPawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<WeaponPawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));

            // Initialize post processor UI handlers.
            PawnSettingUIHandlers.AddHandler(new SetTitlePawnPostProcessorUIHandler());
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<SetNicknamePawnPostProcessor>(new TextField<SetNicknamePawnPostProcessor>(x => x.Format, (x, y) => x.Format = y)));
            PawnSettingUIHandlers.AddHandler(new SetCarriedMedicinePawnPostProcessorUIHandler());
            PawnSettingUIHandlers.AddHandler(new ConditionalPawnPostProcessorUIHandler());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<SetPawnColonistBarOrderingPawnPostProcessor, PawnFitnessDef>());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetApparelPolicyPawnPostProcessor, ApparelPolicy>(
                (m) => Current.Game.outfitDatabase.AllOutfits, (x) => x.label, (x) => x?.Policy?.label ?? "AWA.PolicySelect".Translate(), (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetFoodPolicyPawnPostProcessor, FoodPolicy>(
                (m) => Current.Game.foodRestrictionDatabase.AllFoodRestrictions, (x) => x.label, (x) => x?.Policy?.label ?? "AWA.PolicySelect".Translate(), (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetDrugPolicyPawnPostProcessor, DrugPolicy>(
                (m) => Current.Game.drugPolicyDatabase.AllPolicies, (x) => x.label, (x) => x?.Policy?.label ?? "AWA.PolicySelect".Translate(), (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetReadingPolicyPawnPostProcessor, ReadingPolicy>(
                (m) => Current.Game.readingPolicyDatabase.AllReadingPolicies, (x) => x.label, (x) => x?.Policy?.label ?? "AWA.PolicySelect".Translate(), (pp, po) => pp.Policy = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<MovePawnInColonistBarPawnPostProcessor, MovePawnInColonistBarPawnPostProcessor.MoveToSide>(
                (m) => MovePawnInColonistBarPawnPostProcessor.GetOptions(), (x) => MovePawnInColonistBarPawnPostProcessor.GetLabel(x), (x) => MovePawnInColonistBarPawnPostProcessor.GetLabel(x?.MoveTo), (pp, po) => pp.MoveTo = po));
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<SetSchedulePawnPostProcessor>(x => Find.WindowStack.Add(new EditScheduleWindow(x.Times)), "AWA.ScheduleEdit".Translate()));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetSelfTendPawnPostProcessor, bool>(
                (m) => new bool[] { false, true }, (x) => SetSelfTendPawnPostProcessor.GetLabel(x), (x) => SetSelfTendPawnPostProcessor.GetLabel(x.SelfTend), (pp, po) => pp.SelfTend = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetHostilityResponsePawnPostProcessor, HostilityResponseMode>(
                (m) => (HostilityResponseMode[])Enum.GetValues(typeof(HostilityResponseMode)), (x) => HostilityResponseModeUtility.GetLabel(x), (x) => HostilityResponseModeUtility.GetLabel(x.ResponseMode), (pp, po) => pp.ResponseMode = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetMedicalCarePawnPostProcessor, MedicalCareCategory>(
                (m) => (MedicalCareCategory[])Enum.GetValues(typeof(MedicalCareCategory)), (x) => MedicalCareUtility.GetLabel(x), (x) => MedicalCareUtility.GetLabel(x.MedicalCare), (pp, po) => pp.MedicalCare = po));

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetAllowedAreaPawnPostProcessor, Area>(
                (m) => new List<Area>() { null }.Concat(Current.Game.CurrentMap.areaManager.AllAreas).Where(x => x?.AssignableAsAllowed() ?? true), (x) => x?.Label ?? "AWA.Everywhere".Translate(), (x) => x?.AllowedArea?.Label ?? "AWA.Everywhere".Translate(), (pp, po) => pp.AllowedArea = po));


            // Ideology
            if (ModLister.IdeologyInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<IdeoRolePawnCondition, PreceptDef>(
                    (m) => DefDatabase<PreceptDef>.AllDefs.Where(x => x.roleTags != null && x.roleTags.Any()), x => x.LabelCap, x => x?.RoleDef?.LabelCap ?? "AWA.IdeoRoleSelect".Translate(), (c, s) => c.RoleDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetIdeoRolePawnPostProcessor, PreceptDef>(
                    (m) => DefDatabase<PreceptDef>.AllDefs.Where(x => x.roleTags != null && x.roleTags.Any()), x => x.LabelCap, x => x?.RoleDef?.LabelCap ?? "AWA.IdeoRoleSelect".Translate(), (c, s) => c.RoleDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<IdeoligionPawnCondition, Ideo>(
                    (m) => Find.IdeoManager.IdeosListForReading, x => x.name, x => x.Ideoligion?.name ?? "AWA.IdeoSelect".Translate(), (c, s) => c.Ideoligion = s));
            }

            // Biotech
            if (ModLister.BiotechInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<DevelopmentalStagePawnCondition, DevelopmentalStage>(
                    (m) => ((DevelopmentalStage[])Enum.GetValues(typeof(DevelopmentalStage))).ToList(), x => x.ToString(), x => x?.DevelopmentalStage.ToString() ?? "AWA.StageSelect".Translate(), (c, s) => c.DevelopmentalStage = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<GenePawnCondition, GeneDef>(
                    (m) => DefDatabase<GeneDef>.AllDefs, x => x.LabelCap, x => x?.GeneDef?.LabelCap ?? "AWA.GeneSelect".Translate(), (c, s) => c.GeneDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<XenotypePawnCondition, XenotypeDef>(
                    (m) => DefDatabase<XenotypeDef>.AllDefs, x => x.LabelCap, x => x?.XenotypeDef?.LabelCap ?? "AWA.XenotypeSelect".Translate(), (c, s) => c.XenotypeDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<XenogermPawnCondition, CustomXenogerm>(
                    (m) => Current.Game.customXenogermDatabase.CustomXenogermsForReading, x => x.name, x => x.Xenogerm?.name ?? "AWA.XenotypeSelect".Translate(), (c, s) => c.Xenogerm = s));
            }

            // Royality
            if (ModLister.RoyaltyInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<FavorPawnFitness, Faction>(
                    (m) => new Faction[] { null }.Concat(Find.FactionManager.AllFactionsVisible.Where(x => x.def.royalFavorLabel != null)), x => x?.Name ?? "AWA.Any".Translate(), x => x.Faction?.Name ?? "AWA.Any".Translate(), (c, s) => c.Faction = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<RoyalSeniorityPawnFitness, Faction>(
                    (m) => new Faction[] { null }.Concat(Find.FactionManager.AllFactionsVisible.Where(x => x.def.royalFavorLabel != null)), x => x?.Name ?? "AWA.Any".Translate(), x => x.Faction?.Name ?? "AWA.Any".Translate(), (c, s) => c.Faction = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<RoyaltyTitlePawnCondition, RoyalTitleDef>(
                    (m) => DefDatabase<RoyalTitleDef>.AllDefs, x => x.LabelCap, x => x.TitleDef?.label ?? "AWA.SelectTitle".Translate(), (c, s) => c.TitleDef = s));
            }
        }
    }
}
