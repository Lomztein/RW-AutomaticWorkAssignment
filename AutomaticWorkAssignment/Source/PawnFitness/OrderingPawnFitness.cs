using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class OrderingPawnFitness : PawnSetting, IPawnFitness
    {
        private bool ignore = false;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (ignore) return 0f;
            ignore = true;
            var pawns = specification.GetApplicableOrMinimalPawnsSorted(request.Pawns, request);
            int index = Array.IndexOf(pawns, pawn);
            ignore = false;
            return index;
        }
    }
}
