using Lomzie.AutomaticWorkAssignment.Source;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class PawnSettingDef : Def
    {
        public string? documentationPath;
        public Type settingClass;
        public string type;
        public PawnSettingCategoryDef category;
        public int orderInCategory;

        public bool debug;

        private int GetOrdering()
            => orderInCategory == 0 ? DefDatabase<PawnSettingDef>.AllDefsListForReading.IndexOf(this) : orderInCategory;

        public static List<T> GetSorted<T>() where T : PawnSettingDef
        {
            var defs = DefDatabase<T>.AllDefs.Where(x => !ShouldExclude(x)).ToList();
            defs.Sort(Compare);
            return defs;
        }

        public static bool ShouldExclude(PawnSettingDef def)
        {
            MapPawnsFilter filter = MapWorkManager.GetCurrentMapManager().MapPawnFilter;
            if (def.defName == "Lomzie_IsGuest" && !filter.IncludeGuests)
                return true;
            if (def.defName == "Lomzie_IsSlave" && !filter.IncludeSlaves)
                return true;
            if (def.defName == "Lomzie_IsPrisoner" && !filter.IncludePrisoners)
                return true;
            if (def.defName == "Lomzie_IsDowned" && !filter.IncludeDowned)
                return true;
            if (def.defName == "Lomzie_IsMentallyBroken" && !filter.IncludeMentallyBroken)
                return true;
            if (def.defName == "Lomzie_IsInCryptosleep" && !filter.IncludeCryptosleepers)
                return true;

            if (def.debug && !Prefs.DevMode)
                return true;

            return false;
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
