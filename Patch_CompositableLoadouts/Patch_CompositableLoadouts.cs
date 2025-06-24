using AutomaticWorkAssignment.UI.Generic;
using Inventory;
using Lomzie.AutomaticWorkAssignment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class Patch_CompositableLoadouts : Mod
    {
        public Patch_CompositableLoadouts(ModContentPack content) : base(content)
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<AddTagPawnPostProcessor, Tag>(() => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag));
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<RemoveTagPawnPostProcessor, Tag>(() => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag));
            PawnSettingUIHandlers.AddHandler(new EmptyPawnSettingUIHandler<ClearTagsPawnPostProcessor>());
        }
    }
}
