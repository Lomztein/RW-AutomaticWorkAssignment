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
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    // Don't look at this code. I'm terrible at writing UI.

    public class WorkManagerWindow : MainTabWindow
    {
        private WorkManager _workManager;
        public static WorkManagerWindow Instance;

        // Overall layout
        private const float ListSectionPart = 0.15f;
        private const float MainSectionPart = 0.4f;
        private const float AdvancedSectionPart = 0.45f;
        private const int MarginSize = 4;
        public override Vector2 RequestedTabSize => new Vector2(1400, 400);

        // List section layout.
        private const float ListScrollbarWidth = 16;
        private const float ListElementHeight = 48;
        private float _listHeight;

        private Vector2 _listScrollPosition;
        
        // Main section layout
        private const float ConditionsFitnessSectionPart = 0.2f;
        private const float PriotityListElementWidth = 32;
        private const float RequireFullCapabilitySize = 24;

        // Advanced section layout
        private const float NewFunctionButtonSize = 32;
        private const float SettingsLabelSize = 24;

        private WorkSpecification _current;

        private WorkTypeDef[] _workTypeDefsSorted;

        private Texture2D _resolveNowIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/ResolveNow");
        private Texture2D _autoResolveOnIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/AutoResolveOn");
        private Texture2D _autoResolveOffIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/AutoResolveOff");
        private Texture2D _excludePawnsIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/ExcludePawns");
        private Texture2D _openImportExportIcon = ContentFinder<Texture2D>.Get("UI/AutomaticWorkAssignment/OpenImportExport");

        private Texture2D _toggleOnIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
        private Texture2D _toggleOffIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

        private Vector2 _iconImageSize = new Vector2(24f, 24f);

        public override void PreOpen()
        {
            base.PreOpen();
            _workManager = Current.Game.GetComponent<WorkManager>();

            var workList = DefDatabase<WorkTypeDef>.AllDefs.ToList();
            workList.SortBy(x => x.naturalPriority);
            _workTypeDefsSorted = workList.ToArray();

            Instance = this;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DoListSectionContents(GetMajorSectionRect(inRect, ListSectionPart, 0f));

            if (_current != null)
            {
                DoMainSectionContents(GetMajorSectionRect(inRect, MainSectionPart, ListSectionPart));
                DoAdvancedSectionContents(GetMajorSectionRect(inRect, AdvancedSectionPart, ListSectionPart + MainSectionPart));
            }
        }


        private void DoListSectionContents(Rect rect)
        {
            Widgets.DrawWindowBackground(rect);
            List<WorkSpecification> workSpecifications = _workManager.WorkList;

            Rect listRect = Utils.GetSubRectFraction(rect, Vector3.zero, new Vector2(1f, 0.9f));
            Rect buttomsRect = Utils.GetSubRectFraction(rect, new Vector2(0f, 0.9f), Vector2.one);

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

                Widgets.Label(labelRect, new GUIContent($"{work.Name}"));
                Widgets.Label(assignedRect, new GUIContent("AWA.PawnsAssigned".Translate(_workManager.GetCountAssignedTo(work), work.GetTargetWorkers())));

                if (_current == work)
                    Widgets.DrawHighlight(row);

                Text.Anchor = TextAnchor.UpperLeft;
                
                TooltipHandler.TipRegion(row, () => "AWA.PawnsAssignedTip".Translate("    " + string.Join("\n    ", _workManager.GetPawnsAssignedTo(work))), 10371037);

                Rect rearrangeRect = Utils.GetSubRectFraction(jobRect, new Vector2(0.75f, 0f), new Vector2(0.875f, 1f));
                int movement = DoVerticalRearrangeButtons(rearrangeRect);
                if (movement != 0)
                {
                    _workManager.MoveWorkSpecification(work, GetMovementAmount(movement));
                }
                Rect deleteRect = Utils.GetSubRectFraction(jobRect, new Vector2(0.875f, 0f), new Vector2(1f, 1f));
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
            if (Widgets.ButtonImageWithBG(autoResolveButtonRect, _workManager.RefreshEachDay ? _autoResolveOnIcon : _autoResolveOffIcon, _iconImageSize))
                _workManager.RefreshEachDay = !_workManager.RefreshEachDay;
            if (Widgets.ButtonImageWithBG(excludePawnsButtonRect, _excludePawnsIcon, _iconImageSize))
                OpenExcludePawnsWindow();
            if (Widgets.ButtonImageWithBG(importFromSaveButtonRect, _openImportExportIcon, _iconImageSize))
                OpenImportFromSaveWindow();

            TooltipHandler.TipRegion(resolveNowButtonRect, "AWA.ResolveAssignmentsNowTip".Translate());
            TooltipHandler.TipRegion(autoResolveButtonRect, "AWA.AutoResolveTip".Translate(_workManager.RefreshEachDay.ToString()));
            TooltipHandler.TipRegion(excludePawnsButtonRect, "AWA.ExcludePawnsTip".Translate());
            TooltipHandler.TipRegion(importFromSaveButtonRect, "AWA.SaveLoadTip".Translate());
        }

        private void OpenExcludePawnsWindow()
        {
            if (!Find.WindowStack.IsOpen<ExcludeColonistsWindow>())
            {
                Find.WindowStack.Add(new ExcludeColonistsWindow());
            }
        }

        private void OpenImportFromSaveWindow()
        {
            FloatMenuOption save = new FloatMenuOption("AWA.Save".Translate(), () => Find.WindowStack.Add(new Dialog_SaveConfigFileList()));
            FloatMenuOption load = new FloatMenuOption("AWA.Load".Translate(), () => Find.WindowStack.Add(new Dialog_LoadConfigFileList()));
            FloatMenuOption import = new FloatMenuOption("AWA.Import".Translate(), () => Find.WindowStack.Add(new Dialog_ImportConfigFileList()));
            FloatMenuOption openFolder = new FloatMenuOption("AWA.OpenFolder".Translate(), () => Process.Start(IO.GetConfigDirectory().FullName));
            Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { save, load, import, openFolder }));
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
                Rect sectionRect;

                // Basic info
                sectionRect = Utils.GetSubRectFraction(rect, Vector2.zero, new Vector2(1f, 0.4f));
                Widgets.DrawWindowBackground(sectionRect);
                sectionRect = Utils.ShrinkByMargin(sectionRect, MarginSize);
                DoBasicInfoContents(sectionRect);

                // Priority
                sectionRect = Utils.GetSubRectFraction(rect, new Vector2(0f, 0.4f), new Vector2(1f, 1));
                Widgets.DrawWindowBackground(sectionRect);
                sectionRect = Utils.ShrinkByMargin(sectionRect, MarginSize);
                DoPriorityContents(sectionRect);
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
            DoPawnAmountContents(minRects.labelRect, minRects.contentRect, "AWA.LabelMinWorkers".Translate(), _current.MinWorkers, (x) => _current.MinWorkers = x);

            // Target workers
            Rect maxRect = Utils.GetSubRectFraction(firstRow, new Vector2(0f, 0.66f), new Vector2(1f, 1f));
            maxRect = Utils.ShrinkByMargin(maxRect, MarginSize);
            var maxRects = Utils.GetLabeledContentWithFixedLabelSize(maxRect, labelWidth);
            DoPawnAmountContents(maxRects.labelRect, maxRects.contentRect, "AWA.LabelTargetWorkers".Translate(), _current.TargetWorkers, (x) => _current.TargetWorkers = x);

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
            Rect sectionRect;

            // Fitness
            sectionRect = Utils.GetSubRectFraction(rect, new Vector2(0f, 0f), new Vector2(0.33f, 1f));
            Widgets.DrawWindowBackground(sectionRect);
            sectionRect = Utils.ShrinkByMargin(sectionRect, MarginSize);
            DoFitnessContents(sectionRect);

            // Conditions
            sectionRect = Utils.GetSubRectFraction(rect, new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
            Widgets.DrawWindowBackground(sectionRect);
            sectionRect = Utils.ShrinkByMargin(sectionRect, MarginSize);
            DoConditionsContents(sectionRect);

            // Post process
            sectionRect = Utils.GetSubRectFraction(rect, new Vector2(0.66f, 0f), new Vector2(1f, 1f));
            Widgets.DrawWindowBackground(sectionRect);
            sectionRect = Utils.ShrinkByMargin(sectionRect, MarginSize);
            DoPostProcessContents(sectionRect);
        }

        float _priorityListWidth = 0f;
        Vector2 _priorityListPosition = Vector2.zero;
        private void DoPriorityContents(Rect sectionRect)
        {
            DoHeader(sectionRect, $"<-------- {"AWA.PriorityHigher".Translate()} | {"AWA.PriorityLower".Translate()} -------->");

            sectionRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0, 0.1f), Vector2.one);

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

            Text.Anchor = TextAnchor.MiddleRight;
            Rect requireCapabilityRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0.5f, 0f), new Vector2(1f, 0.1f));
            requireCapabilityRect.height = RequireFullCapabilitySize;
            (Rect labelRect, Rect buttonRect) = Utils.GetLabeledContentWithFixedLabelSize(requireCapabilityRect, requireCapabilityRect.width - RequireFullCapabilitySize);
            labelRect.width -= MarginSize;
            Widgets.Label(labelRect, "AWA.LabelRequireFullCapability".Translate());
            if (Widgets.ButtonImage(buttonRect, _current.RequireFullPawnCapability ? _toggleOnIcon : _toggleOffIcon))
                _current.RequireFullPawnCapability = !_current.RequireFullPawnCapability;
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(requireCapabilityRect, "AWA.LabelRequireFullCapabilityTip".Translate());
        }

        private void DrawPriority(Rect inRect, PawnWorkPriorities priorities, WorkTypeDef workDef)
        {
            inRect = Utils.ShrinkByMargin(inRect, MarginSize / 2);
            Rect labelRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(1f, 0.6f));

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

            Rect rearrangeRect = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0.6f), new Vector2(1f, 0.8f));
            int movement = DoHorizontalRearrangeButtons(rearrangeRect);
            if (movement != 0)
            {
                priorities.MovePriority(workDef, GetMovementAmount(movement));
            }

            Rect deletePart = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0.8f), new Vector2(1f, 1f));
            if (Widgets.ButtonText(deletePart, "X"))
            {
                priorities.RemovePriority(workDef);
            }
        }

        // TODO: Look into options for reducing heavy code duplication of fitness, condition, post process drawing functions.
        private float _fitnessListHeight;
        private Vector2 _fitnessListPosition = Vector2.zero;
        private void DoFitnessContents(Rect sectionRect)
        {
            DoHeader(sectionRect, "AWA.HeaderFitness".Translate());
            sectionRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0, 0.1f), Vector2.one);

            DoPawnSettingList(sectionRect, typeof(PawnFitnessDef), "AWA.FunctionAdd".Translate(), ref _fitnessListHeight, ref _fitnessListPosition,
                () => _current.Fitness,
                (x) => _current.Fitness.Add(x as IPawnFitness),
                (setting, movement) => _current.MoveFitness(setting as IPawnFitness, movement),
                (setting) => _current.DeleteFitness(setting as IPawnFitness)
                );
        }

        private static void DoAdvancedSectionMoveDeleteButtons (Rect inRect, Action<int> onMovement, Action onDelete)
        {
            Rect moveUpRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(0.33f, 1f));
            Rect moveDownRect = Utils.GetSubRectFraction(inRect, new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
            Rect deleteRect = Utils.GetSubRectFraction(inRect, new Vector2(0.66f, 0f), new Vector2(1f, 1f));

            if (onMovement != null)
            {
                if (Widgets.ButtonText(moveUpRect, "/\\"))
                    onMovement(GetMovementAmount(-1));
                if (Widgets.ButtonText(moveDownRect, "\\/"))
                    onMovement(GetMovementAmount(1));
            }

            if (onDelete != null)
            {
                if (Widgets.ButtonText(deleteRect, "X"))
                    onDelete();
            }
        }

        private float _conditionListHeight;
        private Vector2 _conditionListPosition;
        private void DoConditionsContents(Rect sectionRect)
        {
            DoHeader(sectionRect, "AWA.HeaderConditions".Translate());
            sectionRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0, 0.1f), Vector2.one);

            DoPawnSettingList(sectionRect, typeof(PawnConditionDef), "AWA.ConditionAdd".Translate(), ref _conditionListHeight, ref _conditionListPosition,
                () => _current.Conditions,
                (x) => _current.Conditions.Add(x as IPawnCondition),
                null,
                (setting) => _current.DeleteCondition(setting as IPawnCondition)
                );
        }

        public static void DoPawnSettingList(Rect inRect, Type settingDefType, string newSettingLabel, ref float listHeight, ref Vector2 listPosition, Func<IEnumerable<IPawnSetting>> settingGetter, Action<IPawnSetting> onNewSetting, Action<IPawnSetting, int> onMoveSetting, Action<IPawnSetting> onDeleteSetting)
        {
            var settings = settingGetter();

            var height = listHeight;
            var scrollView = new Rect(0f, 0f, inRect.width, height);
            if (height > inRect.height)
                scrollView.width -= ListScrollbarWidth;

            Widgets.BeginScrollView(inRect, ref listPosition, scrollView);
            var scrollContent = scrollView;

            Widgets.BeginGroup(scrollContent);
            var cur = Vector2.zero;
            var i = 0;

            foreach (IPawnSetting setting in settings)
            {
                float x = MarginSize / 2f;
                float width = scrollView.width - MarginSize / 2;

                Rect labelRect = new Rect(x, cur.y, width, SettingsLabelSize);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, GetSettingLabel(setting));
                Text.Anchor = TextAnchor.UpperLeft;

                Action<int> onMove = null;
                Action onDelete = null;

                if (onMoveSetting != null)
                    onMove = (m) => onMoveSetting(setting, m);
                if (onDeleteSetting != null)
                    onDelete = () => onDeleteSetting(setting);

                Rect buttonsRect = Utils.GetSubRectFraction(labelRect, new Vector2(0.6f, 0f), Vector2.one);
                DoAdvancedSectionMoveDeleteButtons(buttonsRect, onMove, onDelete);

                float rowHeight = PawnSettingUIHandlers.Handle(new Vector2(x, cur.y + labelRect.height), width, setting);

                var row = new Rect(0, cur.y, inRect.width, rowHeight + labelRect.height);
                Widgets.DrawHighlightIfMouseover(row);
                TooltipHandler.TipRegion(row, setting.Description);

                if (i++ % 2 == 1) Widgets.DrawAltRect(row);

                cur.y += labelRect.height + rowHeight;
            }

            if (onNewSetting != null)
            {
                // row for new function.
                var newRect = new Rect(0f, cur.y, inRect.width, NewFunctionButtonSize);
                Widgets.DrawHighlightIfMouseover(newRect);
                if (i % 2 == 1) Widgets.DrawAltRect(newRect);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(newRect, new GUIContent(newSettingLabel));
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonInvisible(newRect))
                {
                    var defs = GenDefDatabase.GetAllDefsInDatabaseForDef(settingDefType).Cast<PawnSettingDef>();
                    FloatMenuUtility.MakeMenu(defs, x => x.LabelCap, x => () => onNewSetting(PawnSetting.CreateFrom<IPawnSetting>(x)));
                }

                cur.y += NewFunctionButtonSize;
            }

            listHeight = cur.y;

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public static string GetSettingLabel(IPawnSetting setting)
        {
            if (Find.Selector.NumSelected == 1 && Find.Selector.AnyPawnSelected)
            {
                Pawn selectedPawn = Find.Selector.SelectedPawns.First();
                return $"{setting.Label}: {GetSettingLabelValue(setting, Instance._current, selectedPawn)}";
            }
            return setting.Label;
        }

        private static string GetSettingLabelValue(IPawnSetting setting, WorkSpecification spec, Pawn pawn)
        {
            ResolveWorkRequest req = WorkManager.Instance.MakeDefaultRequest();

            if (setting is IPawnCondition cond) return cond.IsValid(pawn, spec, req).ToString();
            if (setting is IPawnFitness fit) return fit.CalcFitness(pawn, spec, req).ToString("0.##");
            return string.Empty;
        }


        private static int GetMovementAmount(int sign)
            => Input.GetKey(KeyCode.LeftShift) ? sign * 1000 : sign;

        private float _postProcessorListHeight;
        private Vector2 _postProcessorListPosition;
        private void DoPostProcessContents(Rect sectionRect)
        {
            DoHeader(sectionRect, "AWA.HeaderPostProcessors".Translate());
            sectionRect = Utils.GetSubRectFraction(sectionRect, new Vector2(0, 0.1f), Vector2.one);

            DoPawnSettingList(sectionRect, typeof(PawnPostProcessorDef), "AWA.PostProcessorAdd".Translate(), ref _postProcessorListHeight, ref _postProcessorListPosition,
                () => _current.PostProcessors,
                (x) => _current.PostProcessors.Add(x as IPawnPostProcessor),
                (x, m) => _current.MovePostProcessor(x as IPawnPostProcessor, m),
                (setting) => _current.DeletePostProcessor(setting as IPawnPostProcessor)
                );
        }

        // TODO: Move handling of each type into own class.
        private string _pawnAmountBuffer;
        private void DoPawnAmountContents(Rect labelRect, Rect contentRect, string label, IPawnAmount pawnAmount, Action<IPawnAmount> newPawnAmountType)
        {
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect amountRect = Utils.GetSubRectFraction(contentRect, Vector2.zero, new Vector2(0.8f, 1f));
            Rect toggleRect = Utils.GetSubRectFraction(contentRect, new Vector2(0.8f, 0f), Vector2.one);

            Widgets.Label(labelRect, label);
            if (pawnAmount is IntPawnAmount intPawnAmount)
            {
                _pawnAmountBuffer = intPawnAmount.Value.ToString();
                Widgets.TextFieldNumeric(amountRect, ref intPawnAmount.Value, ref _pawnAmountBuffer);
                intPawnAmount.Value = int.Parse(_pawnAmountBuffer);
            }
            if (pawnAmount is PercentagePawnAmount percentagePawnAmount)
            {
                _pawnAmountBuffer = (percentagePawnAmount.Percentage * 100f).ToString();
                Widgets.TextFieldNumeric(amountRect, ref percentagePawnAmount.Percentage, ref _pawnAmountBuffer, 0f, 100f);
                percentagePawnAmount.Percentage = float.Parse(_pawnAmountBuffer) / 100f;
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
                _pawnAmountBuffer = buildingPawnAmount.Multiplier.ToString();
                Widgets.TextFieldNumeric(multRect, ref buildingPawnAmount.Multiplier, ref _pawnAmountBuffer);
                buildingPawnAmount.Multiplier = float.Parse(_pawnAmountBuffer);
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

        private void DoHeader(Rect inRect, string header)
        {
            Rect headerRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(1f, 0.1f));
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawMenuSection(headerRect);
            Widgets.Label(headerRect, header);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Rect GetMajorSectionRect(Rect inRect, float fraction, float moveByFraction)
        {
            Rect newRect = new Rect(inRect);
            newRect.width *= fraction;
            newRect.x += inRect.width * moveByFraction;
            return newRect;
        }

        private void HighlightAssignees(WorkSpecification workSpec)
        {
            var pawns = WorkManager.Instance.GetPawnsAssignedTo(workSpec);
            LookTargetsUtility.TryHighlight(new LookTargets(pawns));
        }
    }
}
