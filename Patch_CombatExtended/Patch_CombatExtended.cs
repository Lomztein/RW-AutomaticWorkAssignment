using AutomaticWorkAssignment.UI.Generic;
using CombatExtended;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CombatExtended
{
    public class Patch_CombatExtended : Mod
    {
        public Patch_CombatExtended(ModContentPack content) : base(content)
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetLoadoutPawnPostProcessor, Loadout>(
                () => LoadoutManager.Loadouts, x => x.label, x => x?.Loadout?.label ?? "Select CE Loadout", (x, y) => x.Loadout = y
                ));
        }
    }
}