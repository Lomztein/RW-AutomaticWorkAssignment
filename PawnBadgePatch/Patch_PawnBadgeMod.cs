using Lomzie.AutomaticWorkAssignment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.PawnBadge
{
    public class Patch_PawnBadgeMod : Mod
    {
        public Patch_PawnBadgeMod(ModContentPack content) : base(content)
        {
            PawnSettingUIHandlers.AddHandler(new SetBadgePawnPostProcessorUIHandler());
        }
    }
}
