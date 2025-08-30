using Lomzie.AutomaticWorkAssignment.Source;
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

        public bool requireColonists;
        public bool requireGuests;
        public bool requireSlaves;
        public bool requirePrisoners;

        private int GetOrdering()
            => orderInCategory == 0 ? DefDatabase<PawnSettingDef>.AllDefsListForReading.IndexOf(this) : orderInCategory;

        public static List<T> GetSorted<T>() where T : PawnSettingDef
        {
            var defs = DefDatabase<T>.AllDefs.Where(IsAvailable).ToList();
            defs.Sort(Compare);
            return defs;
        }

        // "Temporary" solution for disabling select defs under certain conditions.
        // Come up with a more extensible solution if needed.
        private static bool IsAvailable(PawnSettingDef def)
        {
            MapPawnsFilter filter = MapWorkManager.GetCurrentMapManager().MapPawnFilter;
            if (def.requireColonists && !filter.IncludeColonists)
                return false;
            if (def.requireGuests && !filter.IncludeGuests)
                return false;
            if (def.requireSlaves && !filter.IncludeSlaves)
                return false;
            if (def.requirePrisoners && !filter.IncludePrisoners)
                return false;
            return true;
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
