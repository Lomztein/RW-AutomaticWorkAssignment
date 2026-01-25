using Inventory;
using KTrie;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Modular;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public class Patch_CompositableLoadouts : Mod
    {
        public Patch_CompositableLoadouts(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(Initialize);

        }

        private void Initialize()
        {
            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<AddTagPawnPostProcessor>(
                new Picker<AddTagPawnPostProcessor, Tag>((m) => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag),
                new Picker<AddTagPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<RemoveTagPawnPostProcessor>(
                new Picker<RemoveTagPawnPostProcessor, Tag>((m) => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag),
                new Picker<RemoveTagPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<InsertTagPawnPostProcessor>(
                new Picker<InsertTagPawnPostProcessor, Tag>((m) => LoadoutManager.Tags, x => x.name, x => x.Tag?.name ?? "AWA.CL.SelectTag".Translate(), (setting, tag) => setting.Tag = tag),
                new Picker<InsertTagPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state),
                new Splitter<InsertTagPawnPostProcessor>(new Label<InsertTagPawnPostProcessor>(x => "AWA.CL.RelativeTo".Translate(), size: 32), new Picker<InsertTagPawnPostProcessor, Tag>((m) => new Tag[] { null }.Concat(LoadoutManager.Tags), x => x?.name ?? "AWA.None".Translate(), x => x.RelativeTo?.name ?? "AWA.None".Translate(), (setting, tag) => setting.RelativeTo = tag)),
                new Splitter<InsertTagPawnPostProcessor>(new Label<InsertTagPawnPostProcessor>(x => "AWA.CL.Position".Translate()), new Picker<InsertTagPawnPostProcessor, bool>(m => new[] { false, true }, x => x ? "AWA.CL.InsertAfter".Translate() : "AWA.CL.InsertBefore".Translate(), x => x.InsertAfter ? "AWA.CL.InsertAfter".Translate() : "AWA.CL.InsertBefore".Translate(), (pp, x) => pp.InsertAfter = x))));

            PawnSettingUIHandlers.AddHandler(new ModularPawnSettingUIHandler<ClearTagsPawnPostProcessor>(
                new Picker<ClearTagsPawnPostProcessor, LoadoutState>((m) => new LoadoutState[1].Concat(LoadoutManager.States), x => x?.name ?? "AWA.Default".Translate(), x => x.State?.name ?? "AWA.Default".Translate(), (setting, state) => setting.State = state)));
        }
    }
}
