using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class HealthPawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.health.summaryHealth.SummaryHealthPercent ?? 0;
    }
}
