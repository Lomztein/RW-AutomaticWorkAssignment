using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class SlavesPawnAmount : IPawnAmount
    {
        public float Multiplier = 0.25f;

        private Cache<int> _cache = new Cache<int>();

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps().SelectMany(x => x.mapPawns.SlavesOfColonySpawned).Count());
            return _cache.Set(value);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
