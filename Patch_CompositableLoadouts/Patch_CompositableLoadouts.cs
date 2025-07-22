using AutomaticWorkAssignment.UI.Generic;
using Inventory;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
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
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<AddTagPawnPostProcessor>(
                new Picker<AddTagPawnPostProcessor, Tag>((m) => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag),
                new Picker<AddTagPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<RemoveTagPawnPostProcessor>(
                new Picker<RemoveTagPawnPostProcessor, Tag>((m) => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag),
                new Picker<RemoveTagPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ClearTagsPawnPostProcessor>(
                new Picker<ClearTagsPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));
        }
    }
}
