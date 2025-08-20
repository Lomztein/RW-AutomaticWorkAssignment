using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI.Dialogs;
using Lomzie.AutomaticWorkAssignment.UI.Windows;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    // Don't look at this code. I'm terrible at writing UI.
    // I did look ¯\_(ツ)_/¯

    public class WorkManagerWindow : MainTabWindow
    {
        private MapWorkManager _workManager;
        public static WorkManagerWindow Instance;

        // Overall layout
        private float ListSectionWidth => AutomaticWorkAssignmentSettings.ManagerListSectionWidth;
        private float MainSectionWidth => AutomaticWorkAssignmentSettings.ManagerMainSectionWidth;
        private float AdvancedSectionWidth => AutomaticWorkAssignmentSettings.ManagerSettingsSectionWidth;

        private float WindowWidth => ListSectionWidth + MainSectionWidth + AdvancedSectionWidth;
        private float WindowHeight => AutomaticWorkAssignmentSettings.ManagerWindowHeight;


        private const int MarginSize = 4;
        public override Vector2 RequestedTabSize =>
            new Vector2(
                WindowWidth,
                WindowHeight
                );

        // List section layout.
        private static float ListScrollbarWidth => GUI.skin.verticalScrollbar.fixedWidth + 1;
        private const float ListElementHeight = 48;
        private float _listHeight;

        private Vector2 _listScrollPosition;
        private string _search;

        // Main section layout
        private const float PriotityListElementWidth = 32;
        private static float InputSize => AutomaticWorkAssignmentSettings.UIInputSizeBase;
        private static float ButtonSize => AutomaticWorkAssignmentSettings.UIButtonSizeBase;

        // Advanced section layout
        private const float NewFunctionButtonSize = 32;
        private const float SettingsLabelSize = 24;

        private WorkSpecification _current;
        public static WorkSpecification CurrentRenderSpec => Instance._current;
        public static IPawnSetting CurrentRenderSetting { get; private set; }

        private WorkTypeDef[] _workTypeDefsSorted;

        private Texture2D _resolveNowIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/ResolveNow");
        private Texture2D _autoResolveOnIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/AutoResolveOn");
        private Texture2D _autoResolveOffIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/AutoResolveOff");
        private Texture2D _excludePawnsIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/ExcludePawns");
        private Texture2D _openImportExportIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/OpenImportExport");

        private Texture2D _toggleOnIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
        private Texture2D _toggleOffIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

        private Texture2D _searchIcon = ContentFinder<Texture2D>.Get("UI/Widgets/Search");

        private Vector2 _iconImageSize = new Vector2(24f, 24f);

        private string _minPawnAmountBuffer;
        private string _targetPawnAmountBuffer;


        public WorkManagerWindow()
        {
            fitnessConfigurationColumn = new ConfigurationColumnData<IPawnFitness>(
                () => _current.Fitness,
                (fitness) => _current.DeleteFitness(fitness),
                (fitness, index) => _current.MoveFitness(fitness, index));
            conditionsConfigurationColumn = new ConfigurationColumnData<IPawnCondition>(
                () => _current.Conditions,
                (condition) => _current.DeleteCondition(condition),
                (condition, index) => _current.MoveCondition(condition, index));
            postProcessorsConfigurationColumn = new ConfigurationColumnData<IPawnPostProcessor>(
                () => _current.PostProcessors,
                (postProcessor) => _current.DeletePostProcessor(postProcessor),
                (postProcessor, index) => _current.MovePostProcessor(postProcessor, index));
        }
        public override void PreOpen()
        {
            base.PreOpen();
            _workManager = MapWorkManager.GetManager(Find.CurrentMap);

            var workList = DefDatabase<WorkTypeDef>.AllDefs.ToList();
            workList.SortBy(x => x.naturalPriority);
            _workTypeDefsSorted = workList.ToArray();

            Clipboard.Clear();

            Instance = this;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var initialFont = Text.Font;
            Text.Font = GameFont.Small;
            (Rect list, Rect remainder) = Utils.SplitRectHorizontalLeft(inRect, ListSectionWidth);
            (Rect header, Rect listRemainder) = Utils.SplitRectVerticalUpper(list, ButtonSize);

            (Rect moreSettings, Rect search) = Utils.SplitRectHorizontalLeft(header, ButtonSize);
            Text.Anchor = TextAnchor.MiddleCenter;
            if (Widgets.ButtonText(moreSettings, "***"))
            {
                FloatMenuOption setParent = new FloatMenuOption("AWA.SelectMapParent".Translate(), SelectMapParent);
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption> { setParent }));
            }
            Text.Anchor = TextAnchor.UpperLeft;

            if (_workManager.ParentMap != null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(inRect, "AWA.MapParentedLabel".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            // Header section
            if (Clipboard.Contains<WorkSpecification>())
            {
                (Rect newSearch, Rect pasteRect) = Utils.SplitRectHorizontalRight(search, ButtonSize);
                search = newSearch;

                if (Widgets.ButtonImage(pasteRect, TexButton.Paste))
                    _workManager.AddWorkSpecification(Clipboard.Paste<WorkSpecification>());
            }
            (Rect inputRect, Rect iconRect) = Utils.SplitRectHorizontalRight(search, ButtonSize);
            _search = Widgets.TextField(inputRect, _search);
            Widgets.DrawTextureFitted(iconRect, _searchIcon, 0.8f);

            DoListSectionContents(listRemainder);

            if (_current != null)
            {
                (Rect mainRect, Rect mainRemainder) = Utils.SplitRectHorizontalLeft(remainder, MainSectionWidth);
                DoMainSectionContents(mainRect);
                DoAdvancedSectionContents(mainRemainder);
            }

            Text.Font = initialFont;
        }

        private void SelectMapParent()
        {
            Map currentMap = Find.CurrentMap;
            MapWorkManager workManager = currentMap.GetComponent<MapWorkManager>();

            var allMaps = Find.Maps;
            var validMaps = allMaps.Where(x => x != currentMap && x != workManager.ParentMap)
                .Where(x => !workManager.GetAllMaps().Contains(x));

            var selectable = Enumerable.Concat(new List<Map> { null }, validMaps);
            Dialog_SelectMap selectMap = new Dialog_SelectMap(selectable, x => workManager.ParentMap = x);
            Find.WindowStack.Add(selectMap);
        }

        private void DoListSectionContents(Rect rect)
        {
            Widgets.DrawWindowBackground(rect);
            List<WorkSpecification> workSpecifications = _workManager.WorkList;

            if (!string.IsNullOrEmpty(_search))
            {
                // Quick and simple search function. Might do a proper one later.
                string[] parts = _search.Split('|');
                workSpecifications = workSpecifications.
                    Where(x => parts.Any(y => x.Name.ToLower().Contains(y.ToLower())))
                    .ToList();
            }

            (Rect listRect, Rect buttomsRect) = Utils.SplitRectVerticalLower(rect, ButtonSize);

            var height = _listHeight;
            var scrollView = new Rect(0f, 0f, listRect.width, height);
            if (height > listRect.height)
                scrollView.width -= ListScrollbarWidth;

            Widgets.BeginScrollView(listRect, ref _listScrollPosition, scrollView);
            var scrollContent = scrollView;

            Widgets.BeginGroup(scrollContent);
            var cur = Vector2.zero;
            var i = 0;

            foreach (WorkSpecification work in workSpecifications)
            {
                var row = new Rect(0f, cur.y, scrollContent.width, ListElementHeight);
                Widgets.DrawHighlightIfMouseover(row);

                if (i++ % 2 == 1) Widgets.DrawAltRect(row);
                var jobRect = Utils.ShrinkByMargin(row, ListElementHeight * 0.1f);

                if (Mouse.IsOver(row))
                    HighlightAssignees(work);

                Text.Anchor = TextAnchor.MiddleLeft;
                Rect labelRect = Utils.GetSubRectFraction(jobRect, Vector2.zero, new Vector2(1f, 0.5f));
                Rect assignedRect = Utils.GetSubRectFraction(jobRect, new Vector2(0f, 0.5f), new Vector2(1f, 1f));

                Widgets.Label(labelRect, new GUIContent($"{ColorizeIf(work.Name, "grey", work.IsSuspended)}"));
                Widgets.Label(assignedRect, new GUIContent(ColorizeIf("AWA.PawnsAssigned".Translate(_workManager.GetCountAssignedTo(work), work.GetTargetWorkers(_workManager.MakeDefaultRequest())), "grey", work.IsSuspended)));

                if (_current == work)
                    Widgets.DrawHighlight(row);

                Text.Anchor = TextAnchor.UpperLeft;

                TooltipHandler.TipRegion(row, () => "AWA.PawnsAssignedTip".Translate("    " + string.Join("\n    ", _workManager.GetPawnsAssignedTo(work))), 10371037);

                (Rect left, Rect right) = Utils.SplitRectHorizontalRight(jobRect, AutomaticWorkAssignmentSettings.UIButtonSizeBase * 2);
                (Rect _, Rect copyPasteRect) = Utils.SplitRectHorizontalRight(left, ButtonSize * 2);
                (Rect pasteRect, Rect copyRect) = Utils.SplitRectHorizontalLeft(copyPasteRect, ButtonSize);

                if (Clipboard.Contains<WorkSpecification>())
                    if (Widgets.ButtonImage(pasteRect, TexButton.Paste))
                        Clipboard.PasteInto(work);

                if (Widgets.ButtonImage(copyRect, TexButton.Copy))
                    Clipboard.Copy(work);

                // Suspend / alarm
                Text.Anchor = TextAnchor.MiddleCenter;

                Rect suspendAlarm = Utils.GetSubRectFraction(right, Vector2.zero, new Vector2(0.33f, 1f));
                Rect suspend = Utils.GetSubRectFraction(suspendAlarm, Vector2.zero, new Vector2(1f, 0.5f));
                string suspendIcon = "AWA.SuspendCharacter".Translate();
                if (Widgets.ButtonText(suspend, work.IsSuspended ? suspendIcon : $"<color=grey>{suspendIcon}</color>"))
                    work.IsSuspended = !work.IsSuspended;
                TooltipHandler.TipRegion(suspend, "AWA.SuspendWorkTooltip".Translate(work.IsSuspended.ToString()));

                Rect alert = Utils.GetSubRectFraction(suspendAlarm, new Vector2(0f, 0.5f), new Vector2(1f, 1f));
                string alertIcon = "AWA.AlertCharacter".Translate();
                if (Widgets.ButtonText(alert, work.EnableAlert ? alertIcon : $"<color=grey>{alertIcon}</color>"))
                    work.EnableAlert = !work.EnableAlert;
                TooltipHandler.TipRegion(alert, "AWA.AlertWorkTooltip".Translate(work.EnableAlert.ToString()));

                Text.Anchor = TextAnchor.UpperLeft;

                // Rearrange
                Rect rearrangeRect = Utils.GetSubRectFraction(right, new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
                int movement = DoVerticalRearrangeButtons(rearrangeRect);
                if (movement != 0)
                {
                    _workManager.MoveWorkSpecification(work, GetMovementAmount(movement));
                }

                // Delete
                Rect deleteRect = Utils.GetSubRectFraction(right, new Vector2(0.66f, 0f), new Vector2(1f, 1f));
                if (Widgets.ButtonText(deleteRect, "X"))
                    _workManager.DeleteWorkSpecification(work);

                if (Widgets.ButtonInvisible(jobRect))
                {
                    SetCurrent(work);
                }

                cur.y += ListElementHeight;
            }

            // row for new job.
            var newRect = new Rect(0f, cur.y, scrollContent.width, ListElementHeight);
            Widgets.DrawHighlightIfMouseover(newRect);

            if (i % 2 == 1) Widgets.DrawAltRect(newRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(newRect, new GUIContent("AWA.NewWorkSpecification".Translate()));
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(newRect, "AWA.NewWorkSpecificationTip".Translate());

            if (Widgets.ButtonInvisible(newRect))
            {
                SetCurrent(_workManager.CreateNewWorkSpecification());
            }

            cur.y += ListElementHeight;
            _listHeight = cur.y;

            GUI.EndGroup();
            Widgets.EndScrollView();

            Rect resolveNowButtonRect = Utils.GetSubRectFraction(buttomsRect, Vector2.zero, new Vector2(0.25f, 1f));
            Rect autoResolveButtonRect = Utils.GetSubRectFraction(buttomsRect, new Vector2(0.25f, 0f), new Vector2(0.5f, 1f));
            Rect excludePawnsButtonRect = Utils.GetSubRectFraction(buttomsRect, new Vector2(0.5f, 0f), new Vector2(0.75f, 1f));
            Rect importFromSaveButtonRect = Utils.GetSubRectFraction(buttomsRect, new Vector2(0.75f, 0f), new Vector2(1f, 1f));

            if (Widgets.ButtonImageWithBG(resolveNowButtonRect, _resolveNowIcon, _iconImageSize))
                _workManager.ResolveWorkAssignments();
            if (Widgets.ButtonImageWithBG(autoResolveButtonRect, _workManager.DoesAutoResolve ? _autoResolveOnIcon : _autoResolveOffIcon, _iconImageSize))
                OpenAutoResolveFrequencyOptions();
            if (Widgets.ButtonImageWithBG(excludePawnsButtonRect, _excludePawnsIcon, _iconImageSize))
                OpenExcludePawnsWindow();
            if (Widgets.ButtonImageWithBG(importFromSaveButtonRect, _openImportExportIcon, _iconImageSize))
                OpenImportFromSaveWindow();

            TooltipHandler.TipRegion(resolveNowButtonRect, "AWA.ResolveAssignmentsNowTip".Translate());
            TooltipHandler.TipRegion(autoResolveButtonRect, "AWA.AutoResolveTip".Translate(_workManager.ResolveFrequencyDef.LabelCap));
            TooltipHandler.TipRegion(excludePawnsButtonRect, "AWA.ExcludePawnsTip".Translate());
            TooltipHandler.TipRegion(importFromSaveButtonRect, "AWA.SaveLoadTip".Translate());
        }

        private void OpenAutoResolveFrequencyOptions()
        {
            FloatMenuUtility.MakeMenu(AutoResolveFrequencyUtils.Defs, x => x.LabelCap, (x) => () => _workManager.ResolveFrequencyDef = x);
        }

        private void OpenExcludePawnsWindow()
        {
            if (!Find.WindowStack.IsOpen<ExcludeColonistsWindow>())
            {
                Find.WindowStack.Add(new ExcludeColonistsWindow());
            }
        }

        private string Colorize(string text, string colorName)
            => $"<color={colorName}>{text}</color>";
        private string ColorizeIf(string text, string colorName, bool value)
            => value ? Colorize(text, colorName) : text;

        private void OpenImportFromSaveWindow()
        {
            FloatMenuOption save = new FloatMenuOption("AWA.Save".Translate(), () => Find.WindowStack.Add(new Dialog_SaveConfigFileList()));
            FloatMenuOption load = new FloatMenuOption("AWA.Load".Translate(), () => Find.WindowStack.Add(new Dialog_LoadConfigFileList()));
            FloatMenuOption import = new FloatMenuOption("AWA.Import".Translate(), () => Find.WindowStack.Add(new Dialog_ImportSaveConfigFileList()));
            FloatMenuOption openFolder = new FloatMenuOption("AWA.OpenFolder".Translate(), () => Process.Start(IO.GetConfigDirectory().FullName));
            FloatMenuOption resetToDefault = new FloatMenuOption("AWA.ResetToDefault".Translate(), () => Find.WindowStack.Add(new Dialog_Confirm("AWA.ResetManagerToDefault".Translate(), () => _workManager.ResetToDefaults())));
            Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { save, load, import, openFolder, resetToDefault }));
        }

        private int DoVerticalRearrangeButtons(Rect inRect)
        {
            Rect upper = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(1f, 0.5f));
            Rect lower = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0.5f), new Vector2(1f, 1f));

            if (Widgets.ButtonText(upper, "/\\"))
                return -1;
            if (Widgets.ButtonText(lower, "\\/"))
                return 1;
            return 0;
        }

        private int DoHorizontalRearrangeButtons(Rect inRect)
        {
            Rect left = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(0.5f, 1));
            Rect right = Utils.GetSubRectFraction(inRect, new Vector2(0.5f, 0), new Vector2(1f, 1f));

            if (Widgets.ButtonText(left, "<"))
                return -1;
            if (Widgets.ButtonText(right, ">"))
                return 1;
            return 0;
        }

        public void SetCurrent(WorkSpecification current)
        {
            _current = current;
        }

        private void DoMainSectionContents(Rect rect)
        {
            (Rect basicInfoRect, Rect priorityRect) = Utils.SplitRectVerticalUpper(rect, ButtonSize * 6);

            // Basic info
            Widgets.DrawWindowBackground(basicInfoRect);
            basicInfoRect = Utils.ShrinkByMargin(basicInfoRect, MarginSize);
            DoBasicInfoContents(basicInfoRect);

            // Priority
            Widgets.DrawWindowBackground(priorityRect);
            priorityRect = Utils.ShrinkByMargin(priorityRect, MarginSize);
            DoPriorityContents(priorityRect);
        }

        private void DoBasicInfoContents(Rect sectionRect)
        {
            Rect firstRow = Utils.GetSubRectFraction(sectionRect, Vector2.zero, new Vector2(0.5f, 1f));
            int labelWidth = (int)(firstRow.width / 4f);
            Text.Anchor = TextAnchor.MiddleLeft;

            // Name
            Rect nameRect = Utils.GetSubRectFraction(firstRow, Vector2.zero, new Vector2(1f, 0.33f));
            nameRect = Utils.ShrinkByMargin(nameRect, MarginSize);
            var nameRects = Utils.GetLabeledContentWithFixedLabelSize(nameRect, labelWidth);

            Widgets.Label(nameRects.labelRect, "AWA.LabelName".Translate());
            _current.Name = Widgets.TextField(nameRects.contentRect, _current.Name);

            // Min workers
            Rect minRect = Utils.GetSubRectFraction(firstRow, new Vector2(0f, 0.33f), new Vector2(1f, 0.66f));
            minRect = Utils.ShrinkByMargin(minRect, MarginSize);
            var minRects = Utils.GetLabeledContentWithFixedLabelSize(minRect, labelWidth);
            DoPawnAmountContents(minRects.labelRect, minRects.contentRect, "AWA.LabelMinWorkers".Translate(), _current.MinWorkers, _minPawnAmountBuffer, (x) => _current.MinWorkers = x);

            // Target workers
            Rect maxRect = Utils.GetSubRectFraction(firstRow, new Vector2(0f, 0.66f), new Vector2(1f, 1f));
            maxRect = Utils.ShrinkByMargin(maxRect, MarginSize);
            var maxRects = Utils.GetLabeledContentWithFixedLabelSize(maxRect, labelWidth);
            DoPawnAmountContents(maxRects.labelRect, maxRects.contentRect, "AWA.LabelTargetWorkers".Translate(), _current.TargetWorkers, _targetPawnAmountBuffer, (x) => _current.TargetWorkers = x);

            Rect secondRow = Utils.GetSubRectFraction(sectionRect, new Vector2(0.5f, 0f), new Vector2(1f, 1f));
            Text.Anchor = TextAnchor.MiddleLeft;

            // Critical
            Rect criticalRect = Utils.GetSubRectFraction(secondRow, Vector2.zero, new Vector2(1f, 0.33f));
            criticalRect = Utils.ShrinkByMargin(criticalRect, MarginSize);
            var criticalRects = Utils.GetLabeledContentWithFixedLabelSize(criticalRect, labelWidth * 2);
            Widgets.Label(criticalRects.labelRect, "AWA.LabelCritical".Translate());
            if (Widgets.ButtonText(criticalRects.contentRect, _current.IsCritical ? "AWA.Yes".Translate() : "AWA.No".Translate()))
            {
                _current.IsCritical = !_current.IsCritical;
            }
            TooltipHandler.TipRegion(criticalRect, "AWA.LabelCriticalTip".Translate());

            // Specialist
            Rect specialistRect = Utils.GetSubRectFraction(secondRow, new Vector3(0f, 0.33f), new Vector2(1f, 0.66f));
            specialistRect = Utils.ShrinkByMargin(specialistRect, MarginSize);
            var specialistRects = Utils.GetLabeledContentWithFixedLabelSize(specialistRect, labelWidth * 2);
            Widgets.Label(specialistRects.labelRect, "AWA.LabelSpecialist".Translate());
            if (Widgets.ButtonText(specialistRects.contentRect, _current.IsSpecialist ? "AWA.Yes".Translate() : "AWA.No".Translate()))
            {
                _current.IsSpecialist = !_current.IsSpecialist;
            }
            TooltipHandler.TipRegion(specialistRect, "AWA.LabelSpecialistTip".Translate());

            // Commitment
            Rect commitmentRect = Utils.GetSubRectFraction(secondRow, new Vector3(0f, 0.66f), new Vector2(1f, 1f));
            commitmentRect = Utils.ShrinkByMargin(commitmentRect, MarginSize);
            var commitmentRects = Utils.GetLabeledContentWithFixedLabelSize(commitmentRect, labelWidth * 2);
            Widgets.Label(commitmentRects.labelRect, "AWA.LabelCommitment".Translate());
            TooltipHandler.TipRegion(commitmentRect, "AWA.LabelCommitmentTip".Translate());

            Text.Anchor = TextAnchor.UpperCenter;
            Rect sliderLabelRect = Utils.GetSubRectFraction(commitmentRects.contentRect, Vector2.zero, new Vector2(1f, 0.5f));
            Rect sliderRect = Utils.GetSubRectFraction(commitmentRects.contentRect, new Vector2(0f, 0.5f), Vector2.one);
            Widgets.Label(sliderLabelRect, _current.Commitment.ToStringPercent());
            _current.Commitment = Widgets.HorizontalSlider(sliderRect, _current.Commitment, 0f, 1f, roundTo: 0.25f, leftAlignedLabel: null, rightAlignedLabel: null);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoAdvancedSectionContents(Rect rect)
        {
            var listsRect = new RectDivider(rect, GetHashCode(), new(2, 2));
            var cols = listsRect.SplitIn(x: 3);

            foreach (var col in cols)
            {
                Widgets.DrawWindowBackground(col);
            }

            // Fitness
            DoConfigurationColumnContents<IPawnFitness, PawnFitnessDef>(cols[0].Rect.PadXY(1), "AWA.HeaderFitness", "AWA.FunctionAdd", ref fitnessConfigurationColumn);

            // Conditions
            DoConfigurationColumnContents<IPawnCondition, PawnConditionDef>(cols[1].Rect.PadXY(1), "AWA.HeaderConditions", "AWA.ConditionAdd", ref conditionsConfigurationColumn);

            // Post process
            DoConfigurationColumnContents<IPawnPostProcessor, PawnPostProcessorDef>(cols[2].Rect.PadXY(1), "AWA.HeaderPostProcessors", "AWA.PostProcessorAdd", ref postProcessorsConfigurationColumn);
        }


        float _priorityListWidth = 0f;
        Vector2 _priorityListPosition = Vector2.zero;
        private void DoPriorityContents(Rect sectionRect)
        {
            var layout = new RectDivider(sectionRect, GetHashCode(), Vector2.zero);
            var headerRect = DoHeader(ref layout, $"<-------- {"AWA.PriorityHigher".Translate()} | {"AWA.PriorityLower".Translate()} -------->");
            sectionRect = layout.Rect;

            var priorities = _current.Priorities.OrderedPriorities;

            var width = _priorityListWidth;
            var scrollView = new Rect(0f, 0f, sectionRect.width, sectionRect.height);
            if (width > sectionRect.width)
                scrollView.height -= ListScrollbarWidth;

            Widgets.BeginScrollView(sectionRect, ref _priorityListPosition, scrollView);
            var scrollContent = scrollView;

            Widgets.BeginGroup(scrollContent);
            var cur = Vector2.zero;
            var i = 0;

            foreach (WorkTypeDef def in priorities)
            {
                var row = new Rect(cur.x, 0f, PriotityListElementWidth, sectionRect.height);

                if (i++ % 2 == 1) Widgets.DrawAltRect(row);
                DrawPriority(row, _current.Priorities, def);

                cur.x += PriotityListElementWidth;
            }

            // row for new job.
            var newRect = new Rect(cur.x, 0f, PriotityListElementWidth, sectionRect.height);
            Widgets.DrawHighlightIfMouseover(newRect);
            if (i % 2 == 1) Widgets.DrawAltRect(newRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(newRect, new GUIContent("+"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(newRect, "AWA.LabelNewPriorityTip".Translate());

            WorkTypeDef workDef = new WorkTypeDef();

            if (Widgets.ButtonInvisible(newRect))
            {
                FloatMenuUtility.MakeMenu(_workTypeDefsSorted, x => x.labelShort.CapitalizeFirst() + (_current.Priorities.OrderedPriorities.Contains(x) ? " *" : ""), x => () => _current.Priorities.AddPriority(x));
            }

            cur.x += ListScrollbarWidth;
            _priorityListWidth = cur.x;

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Widgets.EndScrollView();

            Rect prioritySettingsRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0.5f, 0f), new Vector2(1f, 1f));
            Rect requireCapabilityRect = new Rect(prioritySettingsRect);
            requireCapabilityRect.height = InputSize;

            DrawPrioritySettingsToggle(requireCapabilityRect, ref _current.RequireFullPawnCapability, "AWA.LabelRequireFullCapability".Translate(), "AWA.LabelRequireFullCapabilityTip".Translate());

            Rect interweaveRect = new Rect(requireCapabilityRect);
            interweaveRect.y += InputSize;
            DrawPrioritySettingsToggle(interweaveRect, ref _current.InterweavePriorities, "AWA.LabelInterweavePriorities".Translate(), "AWA.LabelInterweavePrioritiesTip".Translate());
        }

        private void DrawPrioritySettingsToggle(Rect rect, ref bool value, string label, string description)
        {
            Text.Anchor = TextAnchor.MiddleRight;
            (Rect labelRect, Rect buttonRect) = Utils.GetLabeledContentWithFixedLabelSize(rect, rect.width - InputSize);
            labelRect.width -= MarginSize;
            Widgets.Label(labelRect, label);
            if (Widgets.ButtonImage(buttonRect, value ? _toggleOnIcon : _toggleOffIcon))
                value = !value;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, description);
        }

        private void DrawPriority(Rect inRect, PawnWorkPriorities priorities, WorkTypeDef workDef)
        {
            inRect = Utils.ShrinkByMargin(inRect, MarginSize / 2);

            (Rect labelRect, Rect buttonsRect) = Utils.SplitRectVerticalLower(inRect, ButtonSize * 2);

            Matrix4x4 old = GUI.matrix;

            Utils.RotateAroundPivot(-90f, labelRect.position);
            Text.Anchor = TextAnchor.MiddleLeft;

            labelRect.x -= labelRect.height;
            (labelRect.width, labelRect.height) = (labelRect.height, labelRect.width);

            float hackOffset = inRect.height - inRect.x;
            GUI.matrix *= Matrix4x4.Translate(Vector3.right * -hackOffset);
            Rect hackRect = new Rect(labelRect);
            hackRect.x += hackOffset;

            hackRect.x += MarginSize;
            Widgets.Label(hackRect, workDef.labelShort.CapitalizeFirst());
            GUI.matrix = old;

            (Rect rearrangeRect, Rect deleteRect) = Utils.SplitRectVerticalUpper(buttonsRect, ButtonSize);

            int movement = DoHorizontalRearrangeButtons(rearrangeRect);
            if (movement != 0)
            {
                priorities.MovePriority(workDef, GetMovementAmount(movement));
            }

            if (Widgets.ButtonText(deleteRect, "X"))
            {
                priorities.RemovePriority(workDef);
            }
        }

        struct ConfigurationColumnData<TSetting> where TSetting : IPawnSetting
        {
            public readonly Func<List<TSetting>> getEntries;
            public readonly Action<TSetting> handleDelete;
            public readonly Action<TSetting, int> handleMove;
            public float listHeight;
            public Vector2 listPosition;

            public ConfigurationColumnData(Func<List<TSetting>> getEntries, Action<TSetting> handleDelete, Action<TSetting, int> handleMove)
            {
                this.getEntries = getEntries;
                this.handleDelete = handleDelete;
                this.handleMove = handleMove;
                listHeight = 0;
                listPosition = Vector2.zero;
            }
        }

        private ConfigurationColumnData<IPawnFitness> fitnessConfigurationColumn;
        private ConfigurationColumnData<IPawnCondition> conditionsConfigurationColumn;
        private ConfigurationColumnData<IPawnPostProcessor> postProcessorsConfigurationColumn;
        private void DoConfigurationColumnContents<TSetting, TSettingDef>(
            Rect sectionRect,
            string headerText,
            string addText,
            ref ConfigurationColumnData<TSetting> columnSettings)
            where TSetting : IPawnSetting where TSettingDef : PawnSettingDef
        {
            var layout = new RectDivider(sectionRect, GetHashCode(), Vector2.zero);
            var headerRect = DoHeader(ref layout, headerText.Translate());
            if (Clipboard.Contains<TSetting>() && Widgets.ButtonImage(headerRect.NewCol(ButtonSize, HorizontalJustification.Right), TexButton.Paste))
                columnSettings.getEntries().Add(Clipboard.Paste<TSetting>());

            var locSettings = columnSettings;
            DoPawnSettingList<TSettingDef>(layout, addText.Translate(), ref columnSettings.listHeight, ref columnSettings.listPosition,
                () => locSettings.getEntries().Cast<IPawnSetting>().ToList(),
                (x) => locSettings.getEntries().Add((TSetting)x),
                (setting, movement) => locSettings.handleMove((TSetting)setting, movement),
                (setting) => locSettings.handleDelete((TSetting)setting));
        }

        private static void DoAdvancedSectionMoveDeleteButtons(
            ref RectDivider labelRect,
            bool canMoveUp,
            bool canMoveDown,
            Action<int> onMovement,
            Action onDelete)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            if (onDelete != null)
            {
                var deleteRect = labelRect.NewCol(InputSize, HorizontalJustification.Right);
                if (Widgets.ButtonText(deleteRect, "✕"))
                    onDelete();
            }

            if (onMovement != null)
            {
                if (canMoveUp)
                {
                    var upRect = labelRect.NewCol(InputSize, HorizontalJustification.Right);
                    if (Widgets.ButtonText(upRect, "⇑"))
                        onMovement(GetMovementAmount(-1));
                }
                if (canMoveDown)
                {
                    var downRect = labelRect.NewCol(InputSize, HorizontalJustification.Right);
                    if (Widgets.ButtonText(downRect, "⇓"))
                        onMovement(GetMovementAmount(1));
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void DoPawnSettingList<TDef>(
            Rect inRect,
            string newSettingLabel,
            ref float prevListHeight,
            ref Vector2 listPosition,
            Func<List<IPawnSetting>> settingGetter,
            Action<IPawnSetting> onNewSetting,
            Action<IPawnSetting, int> onMoveSetting,
            Action<IPawnSetting> onDeleteSetting)
            where TDef : PawnSettingDef
        {
            var settings = settingGetter().ToList();

            var scrollInnerContainer = new Rect(0f, 0f, inRect.width, prevListHeight);
            // Leave space for the scrollbar if overflowed
            if (prevListHeight > inRect.height)
                scrollInnerContainer = scrollInnerContainer.Pad(right: ListScrollbarWidth);
            Logger.Message($"Scrollbar width: {ListScrollbarWidth}");

            Widgets.BeginScrollView(inRect, ref listPosition, scrollInnerContainer);
            Widgets.BeginGroup(scrollInnerContainer);

            float width = scrollInnerContainer.width - MarginSize / 2;

            var scrollLayoutAggregator = new RectAggregator(scrollInnerContainer.TopPartPixels(0).Pad(left: MarginSize), Instance.GetHashCode(), new(1, 1));
            for (var i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];

                Rect pawnSettingBlock = DoPawnSetting(
                    ref scrollLayoutAggregator,
                    setting,
                    canMoveUp: i > 0,
                    canMoveDown: i < settings.Count,
                    onMoveSetting, onDeleteSetting);

                if (i % 2 == 1) Widgets.DrawAltRect(pawnSettingBlock.Pad(left: -MarginSize));
            }

            if (onNewSetting != null)
            {
                AddFunctionButton<TDef>(ref scrollLayoutAggregator, newSettingLabel, onNewSetting, settings);
            }

            Text.Anchor = TextAnchor.UpperLeft;
            prevListHeight = scrollLayoutAggregator.Rect.height;
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public static void AddFunctionButton<TDef>(ref RectAggregator layoutAggregator, string newSettingLabel, Action<IPawnSetting> onNewSetting, List<IPawnSetting> settings)
            where TDef : PawnSettingDef
        {
            // row for new function.
            var newRect = layoutAggregator.NewRow(NewFunctionButtonSize).Rect.Pad(left: -MarginSize);
            Widgets.DrawHighlightIfMouseover(newRect);
            if (settings.Count % 2 == 1) Widgets.DrawAltRect(newRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(newRect, new GUIContent(newSettingLabel));
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(newRect))
            {
                var defs = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(TDef)).Cast<TDef>();
                FloatMenuUtility.MakeMenu(defs, x => x.LabelCap, x => () => onNewSetting(PawnSetting.CreateFrom<IPawnSetting>(x)));
            }
        }

        /// <returns>the drawn <see cref="Rect"/> area</returns>
        public static Rect DoPawnSetting(
            ref RectAggregator layout,
            IPawnSetting setting,
            bool canMoveUp,
            bool canMoveDown,
            Action<IPawnSetting, int> onMoveSetting,
            Action<IPawnSetting> onDeleteSetting)
        {
            var localLayout = new RectAggregator(layout.Rect.BottomPart(0), Instance.GetHashCode(), new(1, 1));
            IPawnSetting prevRender = CurrentRenderSetting;
            CurrentRenderSetting = setting;

            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = localLayout.NewRow(SettingsLabelSize);
            Widgets.Label(labelRect, GetSettingLabel(setting));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(labelRect, setting.Description);

            Action<int> onMove = null;
            Action onDelete = null;

            if (onMoveSetting != null)
                onMove = (m) => onMoveSetting(setting, m);
            if (onDeleteSetting != null)
                onDelete = () => onDeleteSetting(setting);

            DoAdvancedSectionMoveDeleteButtons(ref labelRect, canMoveUp, canMoveDown, onMove, onDelete);

            if (Clipboard.Contains(setting.GetType()))
            {
                var pasteRect = labelRect.NewCol(InputSize, HorizontalJustification.Right);
                if (Widgets.ButtonImage(pasteRect, TexButton.Paste))
                    Clipboard.PasteInto(setting);
            }

            var copyRect = labelRect.NewCol(InputSize, HorizontalJustification.Right);
            if (Widgets.ButtonImage(copyRect, TexButton.Copy))
                Clipboard.Copy(setting);

            float rowHeight = PawnSettingUIHandlers.Handle(new Vector2(localLayout.Rect.x, localLayout.Rect.yMax), localLayout.Rect.width, setting);

            var row = localLayout.NewRow(rowHeight);

            CurrentRenderSetting = prevRender;

            var selfRect = layout.NewRow(localLayout.Rect.height);
            Widgets.DrawHighlightIfMouseover(selfRect.Rect.Pad(left: -MarginSize));
            return selfRect;
        }

        public static string GetSettingLabel(IPawnSetting setting)
        {
            if (setting is IPawnPostProcessor)
                return setting.Label;

            if (Find.Selector.NumSelected == 1 && Find.Selector.AnyPawnSelected)
            {
                Pawn selectedPawn = Find.Selector.SelectedPawns.First();
                return $"{setting.Label}: {GetSettingLabelValue(setting, Instance._current, selectedPawn)}";
            }
            return setting.Label;
        }

        private static string GetSettingLabelValue(IPawnSetting setting, WorkSpecification spec, Pawn pawn)
        {
            if (setting == null ||
                spec == null ||
                pawn == null ||
                pawn.Map == null)
            {
                return string.Empty;
            }

            ResolveWorkRequest req = MapWorkManager.GetManager(pawn.Map).MakeDefaultRequest();

            if (setting is IPawnCondition cond) return cond.IsValid(pawn, spec, req).ToString();
            if (setting is IPawnFitness fit) return fit.CalcFitness(pawn, spec, req).ToString("0.##");
            return string.Empty;
        }

        private static int GetMovementAmount(int sign)
            => Input.GetKey(KeyCode.LeftShift) ? sign * 1000 : sign;

        // TODO: Move handling of each type into own class.
        private void DoPawnAmountContents(Rect labelRect, Rect contentRect, string label, IPawnAmount pawnAmount, string pawnAmountBuffer, Action<IPawnAmount> newPawnAmountType)
        {
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect amountRect = Utils.GetSubRectFraction(contentRect, Vector2.zero, new Vector2(0.8f, 1f));
            Rect toggleRect = Utils.GetSubRectFraction(contentRect, new Vector2(0.8f, 0f), Vector2.one);

            Widgets.Label(labelRect, label);
            if (pawnAmount is IntPawnAmount intPawnAmount)
            {
                Widgets.TextFieldNumeric(amountRect, ref intPawnAmount.Value, ref pawnAmountBuffer);
            }
            if (pawnAmount is PercentagePawnAmount percentagePawnAmount)
            {
                float percentage = percentagePawnAmount.Percentage * 100f;
                Widgets.TextFieldNumeric(amountRect, ref percentage, ref pawnAmountBuffer, 0f, 100f);
                percentagePawnAmount.Percentage = percentage / 100f;
            }
            if (pawnAmount is BuildingPawnAmount buildingPawnAmount)
            {
                Rect pickerRect = Utils.GetSubRectFraction(amountRect, Vector2.zero, new Vector2(0.7f, 1f));
                Rect multLabelRect = Utils.GetSubRectFraction(amountRect, new Vector2(0.7f, 0f), new Vector2(0.8f, 1f));
                Rect multRect = Utils.GetSubRectFraction(amountRect, new Vector2(0.8f, 0f), new Vector2(1f, 1f));
                if (Widgets.ButtonText(pickerRect, buildingPawnAmount.BuildingDef?.LabelCap ?? "AWA.Select".Translate()))
                {
                    IEnumerable<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.BuildableByPlayer);
                    FloatMenuUtility.MakeMenu(defs, x => x.label, x => () => buildingPawnAmount.BuildingDef = x);
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(multLabelRect, "x");
                Widgets.TextFieldNumeric(multRect, ref buildingPawnAmount.Multiplier, ref pawnAmountBuffer);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            var pawnAmountDefs = DefDatabase<PawnAmountDef>.AllDefsListForReading;
            PawnAmountDef current = pawnAmountDefs.Find(x => x.defClass.IsInstanceOfType(pawnAmount));
            int currentIndex = pawnAmountDefs.IndexOf(current);
            if (Widgets.ButtonText(toggleRect, current.icon))
            {
                Type newType = pawnAmountDefs[(currentIndex + 1) % pawnAmountDefs.Count].defClass;
                IPawnAmount newPawnAmount = (IPawnAmount)Activator.CreateInstance(newType);
                newPawnAmountType(newPawnAmount);
            }
            TooltipHandler.TipRegion(toggleRect, current.description);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private RectDivider DoHeader(ref RectDivider inRect, string header)
        {
            var headerRect = inRect.NewRow(ButtonSize);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawMenuSection(headerRect);
            Widgets.Label(headerRect, header);
            Text.Anchor = TextAnchor.UpperLeft;
            return headerRect;
        }

        private void HighlightAssignees(WorkSpecification workSpec)
        {
            var pawns = _workManager.GetPawnsAssignedTo(workSpec);
            LookTargetsUtility.TryHighlight(new LookTargets(pawns));
        }
    }
}
