using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class SlavesPawnAmount : PawnAmount
    {
        public float Multiplier = 0.25f;

        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps().SelectMany(x => x.mapPawns.SlavesOfColonySpawned).Count());
            return _cache.Set(value);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
