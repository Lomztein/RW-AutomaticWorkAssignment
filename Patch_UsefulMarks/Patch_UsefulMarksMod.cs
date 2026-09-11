using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Generic;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    public class Patch_UsefulMarksMod : Mod
    {
        public Patch_UsefulMarksMod(ModContentPack content) : base(content)
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<AddMarkPawnPostProcessor, int>(
                _ => UsefulMarksCompatibility.GetMarkerIndices(),
                index => UsefulMarksCompatibility.GetMarkerLabel(index),
                setting => UsefulMarksCompatibility.GetMarkerLabel(setting.MarkerIndex),
                (setting, index) => setting.MarkerIndex = index,
                index => UsefulMarksCompatibility.GetMarkerIcon(index)));

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<RemoveMarkPawnPostProcessor, int>(
                _ => UsefulMarksCompatibility.GetMarkerIndices(),
                index => UsefulMarksCompatibility.GetMarkerLabel(index),
                setting => UsefulMarksCompatibility.GetMarkerLabel(setting.MarkerIndex),
                (setting, index) => setting.MarkerIndex = index,
                index => UsefulMarksCompatibility.GetMarkerIcon(index)));

            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<ClearMarksPawnPostProcessor>());

            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HasMarkPawnCondition, int>(
                _ => UsefulMarksCompatibility.GetMarkerIndices(),
                index => UsefulMarksCompatibility.GetMarkerLabel(index),
                setting => UsefulMarksCompatibility.GetMarkerLabel(setting.MarkerIndex),
                (setting, index) => setting.MarkerIndex = index,
                index => UsefulMarksCompatibility.GetMarkerIcon(index)));
        }
    }
}
