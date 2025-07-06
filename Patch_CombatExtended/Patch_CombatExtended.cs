using AutomaticWorkAssignment.UI.Generic;
using CombatExtended;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CombatExtended
{
    public class Patch_CombatExtended : Mod
    {
        public Patch_CombatExtended(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(Initialize);
        }

        private void Initialize ()
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<SetLoadoutPawnPostProcessor, Loadout>(
                (m) => LoadoutManager.Loadouts, x => x.label, x => x?.Loadout?.label ?? "AWA.CombatExtended.SelectLoadout".Translate(), (x, y) => x.Loadout = y
                ));
        }
    }
}