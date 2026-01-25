using Inventory;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CompositableLoadouts
{
    public static class LoadoutExtensions
    {
        public static void InsertElement(this LoadoutComponent component, LoadoutElement element, int index)
        {
            Pawn pawn = component.parent as Pawn;
            if (!LoadoutManager.PawnsWithTags.TryGetValue(element.Tag, out var value))
            {
                value = new List<Pawn>();
                LoadoutManager.PawnsWithTags.Add(element.Tag, value);
            }
            value.Add(pawn);
            component.Loadout.elements.Insert(index, element);
            foreach (Item item in element.Tag.requiredItems.Where((Item item) => item.Def.IsApparel))
            {
                if (pawn.outfits.CurrentApparelPolicy.filter.Allows(item.Def))
                {
                    continue;
                }
                Messages.Message(Strings.OutfitDisallowsKit(pawn, pawn.outfits.CurrentApparelPolicy, item.Def, element.Tag), pawn, MessageTypeDefOf.CautionInput, historical: false);
                break;
            }
        }
    }
}
