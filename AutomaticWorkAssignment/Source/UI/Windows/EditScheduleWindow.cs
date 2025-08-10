using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class EditScheduleWindow : Window
    {
        private List<TimeAssignmentDef> _timeAssignmentDefs;

        private const int TimeAssignmentSelectorWidth = 191;
        private const int TimeAssignmentSelectorHeight = 65;

        private float _labelSize = 32;
        private float _timeAssignmentCellWidth = 24;
        private float _timeAssignmentCellHeight = 32;

        public override Vector2 InitialSize => new Vector2(_timeAssignmentCellWidth * GenDate.HoursPerDay + Margin * 2, TimeAssignmentSelectorHeight + _timeAssignmentCellHeight + _labelSize + 2);

        public EditScheduleWindow(List<TimeAssignmentDef> timeAssignmentDefs)
        {
            _timeAssignmentDefs = timeAssignmentDefs;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect selectorRect = new Rect(0f, 0f, TimeAssignmentSelectorWidth, TimeAssignmentSelectorHeight);
            if (Widgets.CloseButtonFor(inRect))
            {
                Close();
            }

            TimeAssignmentSelector.DrawTimeAssignmentSelectorGrid(selectorRect);
            for (int h = 0; h < _timeAssignmentDefs.Count; h++)
            {
                Rect rect = new Rect(h * _timeAssignmentCellWidth, TimeAssignmentSelectorHeight / 2f, _timeAssignmentCellWidth, _timeAssignmentCellHeight);
                Rect labelRect = rect;
                labelRect = labelRect.ContractedBy(1);

                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(labelRect, h.ToString());
                Text.Anchor = TextAnchor.UpperLeft;

                Rect cellRect = rect;
                cellRect.y += _labelSize;
                if (DoTimeAssignmentCell(cellRect, _timeAssignmentDefs[h]))
                {
                    _timeAssignmentDefs[h] = TimeAssignmentSelector.selectedAssignment;
                }
            }
        }

        private bool DoTimeAssignmentCell(Rect rect, TimeAssignmentDef timeAssignmentDef)
        {
            rect = rect.ContractedBy(1);
            GUI.DrawTexture(rect, timeAssignmentDef.ColorTexture);
            bool change = false;

            if (Mouse.IsOver(rect) && Input.GetMouseButton(0) && timeAssignmentDef != TimeAssignmentSelector.selectedAssignment)
            {
                SoundDefOf.Designate_DragStandard_Changed_NoCam.PlayOneShotOnCamera();
                change = true;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawBox(rect, 2);
            }
            return change;
        }
    }
}
