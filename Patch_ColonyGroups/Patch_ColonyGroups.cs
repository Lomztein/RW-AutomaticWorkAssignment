using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
using System.Collections.Generic;
using TacticalGroups;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.ColonyGroups
{
    public class Patch_ColonyGroups : Mod
    {
        public Patch_ColonyGroups(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(Initialize);
        }

        private void Initialize()
        {
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<AddToGroupPawnPostProcessor>(
                new Picker<AddToGroupPawnPostProcessor, PawnGroup>(
                    m => TacticUtils.TacticalGroups?.pawnGroups ?? new List<PawnGroup>(),
                    x => x.curGroupName,
                    x => x.Group?.curGroupName ?? "AWA.CG.SelectGroup".Translate(),
                    (setting, group) => setting.Group = group)));
        }
    }
}
