using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class OrderingPawnFitness : PawnSetting, IPawnFitness
    {
        private bool _ignore = false;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (_ignore) return 0f;
            _ignore = true;
            var pawns = specification.GetApplicableOrMinimalPawnsSorted(request.Pawns, request);
            int index = Array.IndexOf(pawns, pawn);
            _ignore = false;
            return index;
        }
    }
}
