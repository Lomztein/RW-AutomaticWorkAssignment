using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class OrderingPawnFitness : PawnSetting, IPawnFitness
    {
        private bool _ignore = false;
        private readonly CacheDict<Pawn, float> _cache = new CacheDict<Pawn, float>();

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (_ignore) return 0f;
            if (!_cache.TryGet(pawn, out float index))
            {
                _ignore = true;
                var pawns = specification.GetApplicableOrMinimalPawnsSorted(request.Pawns, request);
                index = Array.IndexOf(pawns, pawn);
                _cache.Set(pawn, index);
                _ignore = false;
            }
            return index;
        }
    }
}
