using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class WorkCapablePawnFitness : PawnSetting, IPawnFitness
    {
        private readonly List<WorkTypeDef> _workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return _workTypes.Count(x => !pawn.WorkTypeIsDisabled(x));
        }
    }
}
