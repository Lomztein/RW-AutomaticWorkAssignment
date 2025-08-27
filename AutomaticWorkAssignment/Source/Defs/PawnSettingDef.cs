using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class PawnSettingDef : Def
    {
        public string? documentationPath;
        public Type settingClass;
        public string type;
        public PawnSettingCategoryDef category;
        public int orderInCategory;

        private int GetOrdering()
            => orderInCategory == 0 ? DefDatabase<PawnSettingDef>.AllDefsListForReading.IndexOf(this) : orderInCategory;

        public static List<T> GetSorted<T>() where T : PawnSettingDef
        {
            var defs = DefDatabase<T>.AllDefs.ToList();
            defs.Sort(Compare);
            return defs;
        }

        private static int Compare(PawnSettingDef x, PawnSettingDef y)
        {
            int xCat = x.category?.order ?? 0;
            int yCat = y.category?.order ?? 0;

            int catOrder = xCat - yCat;
            if (catOrder != 0)
                return catOrder;

            return x.GetOrdering() - y.GetOrdering();
        }
    }
}
