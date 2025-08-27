using System.Collections.Generic;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class PawnFitnessDef : PawnSettingDef
    {
        public static List<PawnFitnessDef> GetSorted()
            => GetSorted<PawnFitnessDef>();
    }
}
