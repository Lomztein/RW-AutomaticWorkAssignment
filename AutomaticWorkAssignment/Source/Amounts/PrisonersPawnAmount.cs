using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class PrisonersPawnAmount : PawnAmount
    {
        public float Multiplier = 0.25f;

        private Cache<int> _cache = new Cache<int>();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps().SelectMany(x => x.mapPawns.PrisonersOfColony).Count());
            return _cache.Set(value);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
