using System.Collections.Generic;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class PawnPostProcessorDef : PawnSettingDef
    {
        public static List<PawnPostProcessorDef> GetSorted()
            => GetSorted<PawnPostProcessorDef>();
    }
}
