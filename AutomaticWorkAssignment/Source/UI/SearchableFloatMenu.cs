using FloatSubMenus;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public class SearchableFloatMenu : FloatMenu
    {
        public SearchableFloatMenu(List<FloatMenuOption> options) : base(new List<FloatMenuOption>() { new FloatMenuSearch() }.Concat(options).ToList())
        {
        }

        public static void MakeMenu<T>(IEnumerable<T> options, Func<T, string> labelGetter, Func<T, Action> actionGetter)
        {
            List<FloatMenuOption> floatMenuOptions = new();
            foreach (var option in options)
            {
                floatMenuOptions.Add(new FloatMenuOption(labelGetter(option), actionGetter(option)));
            }
            Find.WindowStack.Add(new SearchableFloatMenu(floatMenuOptions));
        }
    }
}
