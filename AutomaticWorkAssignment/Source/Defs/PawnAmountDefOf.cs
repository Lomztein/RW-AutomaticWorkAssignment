using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    [DefOf]
    public static class PawnAmountDefOf
    {
        public static PawnAmountDef Lomzie_IntPawnAmount;
        public static PawnAmountDef Lomzie_PercentagePawnAmount;
        public static PawnAmountDef Lomzie_BuildingPawnAmount;

        static PawnAmountDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnAmountDefOf));
        }
    }
}
