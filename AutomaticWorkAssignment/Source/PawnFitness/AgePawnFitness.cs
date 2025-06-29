using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class AgePawnFitness : PawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return pawn.ageTracker.AgeBiologicalYearsFloat;
            return 0f;
        }
    }
}
