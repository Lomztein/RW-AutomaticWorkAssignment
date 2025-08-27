using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class PawnSettingCategoryDef : Def
    {
        public int order;

        public static List<PawnSettingCategoryDef> GetSorted()
        {
            var defs = DefDatabase<PawnSettingCategoryDef>.AllDefs.ToList();
            defs.Sort(Compare);
            return defs;
        }

        private static int Compare(PawnSettingCategoryDef x, PawnSettingCategoryDef y)
        {
            return x.order - y.order;
        }
    }
}
