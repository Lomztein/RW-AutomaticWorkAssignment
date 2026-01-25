using AutomaticWorkAssignment.UI;
using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.Source.UI.Modular;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Amounts;
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
        private static readonly Uri DOCUMENTATION_BASE_URL = new("https://github.com/Lomztein/RW-AutomaticWorkAssignment/tree/main/Documentation/User/");
        /// <summary>
        /// Resolves the documentation URL for a given base filename. This is implemented this way so that when we migrate to a proper documentation website, only this function need to be modified.
        /// </summary>
        /// <param name="path">The <b>base filename</b> relative to the users documentation root.</param>
        /// <returns>The documentation URL</returns>
        internal static string GetDocumentationUrl(string path) => new Uri(DOCUMENTATION_BASE_URL, path + ".md").ToString();
        /// <summary>
        /// Open the browser for the given help file
        /// </summary>
        /// <param name="path">The <b>base filename</b> relative to the users documentation root.</param>
        internal static void OpenWebDocumentation(string path) => Application.OpenURL(GetDocumentationUrl(path));
        public static AutomaticWorkAssignmentSettings Settings;

        public AutomaticWorkAssignmentMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<AutomaticWorkAssignmentSettings>();
            LongEventHandler.ExecuteWhenFinished(Initialize);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoWindow(inRect);
        }

        public override string SettingsCategory()
            => "Automatic Work Assignment";

        private void Initialize()
        {
            InitializeAmountHandlers();
            InitializeSettingHandlers();
        }

        private void InitializeAmountHandlers()
        {
            PawnAmountUIHandlers.AddHandler(new IntPawnAmountUIHandler());
            PawnAmountUIHandlers.AddHandler(new PercentagePawnAmountUIHandler());
            PawnAmountUIHandlers.AddHandler(new BuildingPawnAmountUIHandler());
            PawnAmountUIHandlers.AddHandler(new StockpilePawnAmountUIHandler());
            PawnAmountUIHandlers.AddHandler(new MultiplierPawnAmountUIHandler<FarmlandPawnAmount>(x => x.Multiplier, (x, y) => x.Multiplier = y));
            PawnAmountUIHandlers.AddHandler(new MultiplierPawnAmountUIHandler<AnimalsPawnAmount>(x => x.Multiplier, (x, y) => x.Multiplier = y));
            PawnAmountUIHandlers.AddHandler(new MultiplierPawnAmountUIHandler<PrisonersPawnAmount>(x => x.Multiplier, (x, y) => x.Multiplier = y));
            PawnAmountUIHandlers.AddHandler(new MultiplierPawnAmountUIHandler<SlavesPawnAmount>(x => x.Multiplier, (x, y) => x.Multiplier = y));
            PawnAmountUIHandlers.AddHandler(new MultiplierPawnAmountUIHandler<GuestsPawnAmount>(x => x.Multiplier, (x, y) => x.Multiplier = y));
            PawnAmountUIHandlers.AddHandler(new CompositePawnAmountUIHandler<MaxPawnAmount>(x => x.InnerAmounts, "AWA.MaxEdit"));
        }

        // I like to show this method to my friends around the campfire to scare them.
        private void InitializeSettingHandlers() {
            // Initialize fitness UI handlers.
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<PassionCountPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<CommitmentPawnFitness>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<WorkCapablePawnFitness>());
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
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HealthConditionSeverityPawnFitness, HediffDef>(
                (m) => new HediffDef[] { null }.Concat(HealthConditionSeverityPawnFitness.GetApplicableHediffDefs()), x => x?.LabelCap ?? "AWA.WorstCase".Translate(), x => x?.Hediff?.LabelCap ?? "AWA.WorstCase".Translate(), (c, s) => c.Hediff = s));

            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<InvertPawnFitness, IPawnFitness, PawnFitnessDef>());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<ConditionPawnFitness, IPawnCondition, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, CountPawnFitness, PawnConditionDef>("AWA.ConditionAdd".Translate()));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, CountPawnsPawnFitness, PawnConditionDef>("AWA.ConditionAdd".Translate()));
            PawnSettingUIHandlers.AddHandler(new ConstantPawnFitnessUIHandler());
            PawnSettingUIHandlers.AddHandler(new FormulaPawnFitnessUIHandler());

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, AggregatePawnFitness, PawnFitnessDef>("AWA.FunctionAdd".Translate()));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnFitness, AveragePawnFitness, PawnFitnessDef>("AWA.FunctionAdd".Translate()));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ModulusPawnFitness>(
                new Nested<ModulusPawnFitness, IPawnFitness, PawnFitnessDef>(x => x.LeftHandSide, (x, setting) => x.LeftHandSide = setting),
                new Label<ModulusPawnFitness>(x => "%"),
                new Nested<ModulusPawnFitness, IPawnFitness, PawnFitnessDef>(x => x.RightHandSide, (x, setting) => x.RightHandSide = setting)
            ));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<IfPawnFitness>(
                new Nested<IfPawnFitness, IPawnCondition, PawnConditionDef>(x => x.IfCondition, (s, n) => s.IfCondition = n, "AWA.ConditionSelect".Translate()),
                new Label<IfPawnFitness>(x => "AWA.Return".Translate()),
                new Nested<IfPawnFitness, IPawnFitness, PawnFitnessDef>(x => x.TrueFitness, (s, n) => s.TrueFitness = n, "AWA.FunctionSelect".Translate()),
                new Label<IfPawnFitness>(x => "AWA.Else".Translate()),
                new Nested<IfPawnFitness, IPawnCondition, PawnConditionDef>(x => x.ElseCondition, (s, n) => s.IfCondition = n, "AWA.ConditionSelect".Translate()),
                new Label<IfPawnFitness>(x => "AWA.Return".Translate()),
                new Nested<IfPawnFitness, IPawnFitness, PawnFitnessDef>(x => x.FalseFitness, (s, n) => s.FalseFitness = n, "AWA.FunctionSelect".Translate())
            ));

            // Initialize condition UI handlers.
            PawnSettingUIHandlers.AddHandler(new CommitmentLimitPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<SlavePawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<GuestPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<MutantPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new SkillLevelPawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new FitnessInRangePawnConditionUIHandler());
            PawnSettingUIHandlers.AddHandler(new CompareFitnessPawnConditionUIHandler());

            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<DownedPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<MentalBreakPawnCondition>());
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<CryptosleepPawnCondition>());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HediffPawnCondition, HediffDef>(
                (m) => DefDatabase<HediffDef>.AllDefs, x => x.LabelCap, x => x?.HediffDef?.LabelCap ?? "AWA.ConditionSelect".Translate(), (c, s) => c.HediffDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<PassionPawnCondition, SkillDef>(
                (m) => DefDatabase<SkillDef>.AllDefs, x => x.LabelCap, x => x?.SkillDef?.LabelCap ?? "AWA.SkillSelect".Translate(), (c, s) => c.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<LearningSaturatedPawnCondition, SkillDef>(
                (m) => DefDatabase<SkillDef>.AllDefs, x => x.LabelCap, x => x?.SkillDef?.LabelCap ?? "AWA.SkillSelect".Translate(), (c, s) => c.SkillDef = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SpecificPawnCondition, Pawn>(
                (m) => MapWorkManager.GetManager(m).GetAllPawns(), x => x.Name.ToString(), x => x?.Pawn?.Name.ToString() ?? "AWA.PawnSelect".Translate(), (c, s) => c.Pawn = s));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<TimeAssignmentCondition, TimeAssignmentDef>(
                (m) => DefDatabase<TimeAssignmentDef>.AllDefs, x => x.LabelCap, x => x.TimeAssignmentDef?.LabelCap ?? "AWA.TimeAssignmentSelect".Translate(), (c, s) => c.TimeAssignmentDef = s));
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
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<WorkTagCapablePawnCondition>(new Toggle<WorkTagCapablePawnCondition>(x => x.RequireAll, (x, y) => x.RequireAll = y, x => "AWA.RequireAll".Translate()), new Lister<WorkTagCapablePawnCondition, WorkTags, WorkTags>((rect, e) => Widgets.Label(rect, e.LabelTranslated().CapitalizeFirst()), AutomaticWorkAssignmentSettings.UILabelSizeBase, x => WorkTagCapablePawnCondition.ValidTags, x => x.LabelTranslated().CapitalizeFirst(), x => x.RequiredCapabilities, (x, y) => y, null)));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<HourBetweenPawnCondition>(
                new Splitter<HourBetweenPawnCondition>(new Label<HourBetweenPawnCondition>(x => "AWA.MinValue".Translate()), new Label<HourBetweenPawnCondition>(x => "AWA.MaxValue".Translate())),
                new Splitter<HourBetweenPawnCondition>(new TextFieldNumeric<float, HourBetweenPawnCondition>(x => x.MinHour, (x, v) => x.MinHour = v), new TextFieldNumeric<float, HourBetweenPawnCondition>(x => x.MaxHour, (x, v) => x.MaxHour = v))));

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, AnyPawnCondition, PawnConditionDef>("AWA.ConditionSelect".Translate()));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, AllPawnCondition, PawnConditionDef>("AWA.ConditionSelect".Translate()));
            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnCondition, HasChildrenPawnCondition, PawnConditionDef>("AWA.ConditionSelect".Translate()));

            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<NotPawnCondition, IPawnCondition, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<AnyPawnPawnCondition, IPawnCondition, PawnConditionDef>());
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<StockpilePawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<ApparelPawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));
            PawnSettingUIHandlers.AddHandler(new ClickablePawnSettingsUIHandler<WeaponPawnCondition>(x => Find.WindowStack.Add(new EditThingFilterWindow(x.ThingFilter)), "AWA.FilterEdit".Translate()));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<IfPawnCondition>(
                new Nested<IfPawnCondition, IPawnCondition, PawnConditionDef>(x => x.IfCondition, (s, n) => s.IfCondition = n, "AWA.ConditionSelect".Translate()),
                new Label<IfPawnCondition>(x => "AWA.Return".Translate(), TextAnchor.MiddleLeft),
                new Nested<IfPawnCondition, IPawnCondition, PawnConditionDef>(x => x.TrueCondition, (s, n) => s.TrueCondition = n, "AWA.ConditionSelect".Translate()),
                new Label<IfPawnCondition>(x => "AWA.Else".Translate(), TextAnchor.MiddleLeft),
                new Nested<IfPawnCondition, IPawnCondition, PawnConditionDef>(x => x.ElseCondition, (s, n) => s.IfCondition = n, "AWA.ConditionSelect".Translate()),
                new Label<IfPawnCondition>(x => "AWA.Return".Translate(), TextAnchor.MiddleLeft),
                new Nested<IfPawnCondition, IPawnCondition, PawnConditionDef>(x => x.FalseCondition, (s, n) => s.FalseCondition = n, "AWA.ConditionSelect".Translate())
            ));


            // Initialize post processor UI handlers.
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<SetTitlePawnPostProcessor>(new TextField<SetTitlePawnPostProcessor>(x => x.Title, (pp, x) => pp.Title = x), new Toggle<SetTitlePawnPostProcessor>(x => x.AllowOverwrite, (pp, x) => pp.AllowOverwrite = x, x => "AWA.AllowOverwrite".Translate())));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<SetNicknamePawnPostProcessor>(new TextField<SetNicknamePawnPostProcessor>(x => x.Format, (pp, x) => pp.Format = x), new Toggle<SetNicknamePawnPostProcessor>(x => x.AllowOverwrite, (pp, x) => pp.AllowOverwrite = x, x => "AWA.AllowOverwrite".Translate())));
            PawnSettingUIHandlers.AddHandler(new SetCarriedMedicinePawnPostProcessorUIHandler());
            PawnSettingUIHandlers.AddHandler(new ConditionalPawnPostProcessorUIHandler());
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<SetPawnColonistBarOrderingPawnPostProcessor, IPawnFitness, PawnFitnessDef>());

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
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<SetSchedulePawnPostProcessor>(new Clickable<SetSchedulePawnPostProcessor>(x => Find.WindowStack.Add(new EditScheduleWindow(x.Times)), x => "AWA.ScheduleEdit".Translate()), 
                new Splitter<SetSchedulePawnPostProcessor>(new Label<SetSchedulePawnPostProcessor>(x => "AWA.Mode".Translate(), TextAnchor.MiddleCenter, 32), new Tooltip<SetSchedulePawnPostProcessor>(x => SetSchedulePawnPostProcessor.GetTooltip(x.Mode), new Picker<SetSchedulePawnPostProcessor, SetSchedulePawnPostProcessor.SetMode>(x => (SetSchedulePawnPostProcessor.SetMode[])Enum.GetValues(typeof(SetSchedulePawnPostProcessor.SetMode)), x => SetSchedulePawnPostProcessor.GetLabel(x), x => SetSchedulePawnPostProcessor.GetLabel(x.Mode), (pp, x) => pp.Mode = x))), 
                new Nested<SetSchedulePawnPostProcessor, IPawnFitness, PawnFitnessDef>(x => x.InnerSetting as IPawnFitness, (pp, x) => pp.InnerSetting  = x, "AWA.OffsetSelect".Translate())));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ShiftSchedulePawnPostProcessor>(new Nested<ShiftSchedulePawnPostProcessor, IPawnFitness, PawnFitnessDef>(x => x.InnerSetting as IPawnFitness, (pp, x) => pp.InnerSetting = x, "AWA.OffsetSelect".Translate())));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetSelfTendPawnPostProcessor, bool>(
                (m) => new bool[] { false, true }, (x) => SetSelfTendPawnPostProcessor.GetLabel(x), (x) => SetSelfTendPawnPostProcessor.GetLabel(x.SelfTend), (pp, po) => pp.SelfTend = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetHostilityResponsePawnPostProcessor, HostilityResponseMode>(
                (m) => (HostilityResponseMode[])Enum.GetValues(typeof(HostilityResponseMode)), (x) => HostilityResponseModeUtility.GetLabel(x), (x) => HostilityResponseModeUtility.GetLabel(x.ResponseMode), (pp, po) => pp.ResponseMode = po));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetMedicalCarePawnPostProcessor, MedicalCareCategory>(
                (m) => (MedicalCareCategory[])Enum.GetValues(typeof(MedicalCareCategory)), (x) => MedicalCareUtility.GetLabel(x), (x) => MedicalCareUtility.GetLabel(x.MedicalCare), (pp, po) => pp.MedicalCare = po));
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ForcePriorityPawnPostProcessor>(new Picker<ForcePriorityPawnPostProcessor, WorkTypeDef>(m => DefDatabase<WorkTypeDef>.AllDefs, x => x.labelShort, x => x.WorkType?.labelShort ?? "AWA.WorkTypeSelect".Translate(), (pp, wt) => pp.WorkType = wt), new TextFieldNumeric<int, ForcePriorityPawnPostProcessor>(x => x.Priority, (pp, p) => pp.Priority = p)));

            PawnSettingUIHandlers.AddHandler(new CompositePawnSettingsUIHandler<IPawnPostProcessor, DoAllPawnPostProcessor, PawnPostProcessorDef>("AWA.PostProcessorAdd".Translate()));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetAllowedAreaPawnPostProcessor, Area>(
                (m) => new List<Area>() { null }.Concat(Current.Game.CurrentMap.areaManager.AllAreas).Where(x => x?.AssignableAsAllowed() ?? true), (x) => x?.Label ?? "AWA.Everywhere".Translate(), (x) => x?.AllowedArea?.Label ?? "AWA.Everywhere".Translate(), (pp, po) => pp.AllowedArea = po));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<DedicatePawnPostProcessor>(new Splitter<DedicatePawnPostProcessor>(
                new TextFieldNumeric<float, DedicatePawnPostProcessor>(x => x.Time, (x, f) => x.Time = f),
                new Picker<DedicatePawnPostProcessor, DedicatePawnPostProcessor.TimeUnit>(m => (DedicatePawnPostProcessor.TimeUnit[])Enum.GetValues(typeof(DedicatePawnPostProcessor.TimeUnit)), x => DedicatePawnPostProcessor.GetLabel(x).Translate(), x => DedicatePawnPostProcessor.GetLabel(x.Unit).Translate(), (pp, en) => pp.Unit = en, buttonSize: AutomaticWorkAssignmentSettings.UIInputSizeBase)),
                new Tooltip<DedicatePawnPostProcessor>(x => "AWA.CurrentDedicatedPawnsTooltip".Translate("\n    " + string.Join("\n    ", x.GetCurrentMapDedicatedPawns().Select(x => x.ToString()))), new Clickable<DedicatePawnPostProcessor>(x => x.ClearCurrentMapDedicatedPawns(), x => "AWA.ClearDedicatedPawns".Translate(x.GetCurrentMapDedicatedPawns().Count())))));

            // Ideology
            if (ModLister.IdeologyInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<CertaintyPawnFitness>());
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<IdeoRolePawnCondition, PreceptDef>(
                    (m) => new PreceptDef[] { null }.Concat(DefDatabase<PreceptDef>.AllDefs.Where(x => x.roleTags != null && x.roleTags.Any())), x => x?.LabelCap ?? "AWA.Any".Translate(), x => x?.RoleDef?.LabelCap ?? "AWA.Any".Translate(), (c, s) => c.RoleDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<IdeoPreceptPawnCondition, PreceptDef>(
                    (m) => DefDatabase<PreceptDef>.AllDefs, x => Utils.GetPreceptLabel(x), x => Utils.GetPreceptLabel(x.PreceptDef) ?? "AWA.IdeoPreceptSelect".Translate(), (c, s) => c.PreceptDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetIdeoRolePawnPostProcessor, PreceptDef>(
                    (m) => DefDatabase<PreceptDef>.AllDefs.Where(x => x.roleTags != null && x.roleTags.Any()), x => x.LabelCap, x => x?.RoleDef?.LabelCap ?? "AWA.IdeoRoleSelect".Translate(), (c, s) => c.RoleDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<IdeoligionPawnCondition, Ideo>(
                    (m) => Find.IdeoManager.IdeosListForReading, x => x.name, x => x.Ideoligion?.name ?? "AWA.IdeoSelect".Translate(), (c, s) => c.Ideoligion = s));
                PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<TreeConnectionPawnCondition>(new ThingTargeter<TreeConnectionPawnCondition>(x => x.Tree, (x, tree) => x.Tree = tree, x => x.TryGetComp<CompTreeConnection>() != null, x => x.Tree?.LabelCap ?? "AWA.Any".Translate())));
            }

            // Biotech
            if (ModLister.BiotechInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<MechanoidCountPawnFitness, PawnKindDef>(x => new PawnKindDef[] { null }.Concat(DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.IsMechanoid)), x => x?.LabelCap ?? "AWA.All".Translate(), x => x.MechanoidDef?.LabelCap ?? "AWA.All".Translate(), (s, d) => s.MechanoidDef = d));
                PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<MechanitorPawnCondition>());

                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<DevelopmentalStagePawnCondition, DevelopmentalStage>(
                    (m) => ((DevelopmentalStage[])Enum.GetValues(typeof(DevelopmentalStage))).ToList(), x => x.ToString(), x => x?.DevelopmentalStage.ToString() ?? "AWA.StageSelect".Translate(), (c, s) => c.DevelopmentalStage = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<GenePawnCondition, GeneDef>(
                    (m) => DefDatabase<GeneDef>.AllDefs, x => x.LabelCap, x => x?.GeneDef?.LabelCap ?? "AWA.GeneSelect".Translate(), (c, s) => c.GeneDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<XenotypePawnCondition, XenotypeDef>(
                    (m) => DefDatabase<XenotypeDef>.AllDefs, x => x.LabelCap, x => x?.XenotypeDef?.LabelCap ?? "AWA.XenotypeSelect".Translate(), (c, s) => c.XenotypeDef = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<XenogermPawnCondition, CustomXenogerm>(
                    (m) => Current.Game.customXenogermDatabase.CustomXenogermsForReading, x => x.name, x => x.Xenogerm?.name ?? "AWA.XenotypeSelect".Translate(), (c, s) => c.Xenogerm = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<MechanoidsPawnCondition, PawnKindDef>(x => new PawnKindDef[] { null }.Concat(DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.IsMechanoid)), x => x?.LabelCap ?? "AWA.Any".Translate(), x => x.MechanoidDef?.LabelCap ?? "AWA.Any".Translate(), (s, d) => s.MechanoidDef = d));
            }

            // Royality
            if (ModLister.RoyaltyInstalled)
            {
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<FavorPawnFitness, Faction>(
                    (m) => new Faction[] { null }.Concat(Find.FactionManager.AllFactionsVisible.Where(x => x.def.royalFavorLabel != null)), x => x?.Name ?? "AWA.Any".Translate(), x => x.Faction?.Name ?? "AWA.Any".Translate(), (c, s) => c.Faction = s));
                PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<RoyalSeniorityPawnFitness, Faction>(
                    (m) => new Faction[] { null }.Concat(Find.FactionManager.AllFactionsVisible.Where(x => x.def.royalFavorLabel != null)), x => x?.Name ?? "AWA.Any".Translate(), x => x.Faction?.Name ?? "AWA.Any".Translate(), (c, s) => c.Faction = s));
                PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<RoyaltyTitlePawnCondition>(
                    new Picker<RoyaltyTitlePawnCondition, Faction>((m) => new Faction[] { null }.Concat(Find.FactionManager.AllFactionsVisible.Where(x => x.def.royalFavorLabel != null)), x => x?.Name ?? "AWA.Any".Translate(), x => x.Faction?.Name ?? "AWA.Any".Translate(), (c, s) => c.Faction = s),
                    new Picker<RoyaltyTitlePawnCondition, RoyalTitleDef>((m) => new RoyalTitleDef[] { null }.Concat(DefDatabase<RoyalTitleDef>.AllDefs), x => x?.LabelCap ?? "AWA.Any".Translate(), x => x.TitleDef?.label ?? "AWA.Any".Translate(), (c, s) => c.TitleDef = s)
                    ));
            }

            // Debug
            PawnSettingUIHandlers.AddHandler(new NestedPawnSettingUIHandler<DebugMessagePawnPostProcessor, IPawnSetting, PawnSettingDef>());
        }
    }
}
