using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class StockpilePawnAmount : IPawnAmount
    {
        public ThingFilter ThingFilter = new();
        public float Multiplier = 0.1f;

        private Cache<int> _cache = new Cache<int>();

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (ThingFilter != null)
            {
                if (_cache.TryGet(out int value))
                    return value;

                value = Mathf.CeilToInt(req.WorkManager.GetAllMaps()
                        .SelectMany(x => x.listerThings.AllThings)
                        .Sum(x => ThingFilter.Allows(x) ? x.stackCount : 0) * Multiplier);

                return _cache.Set(value);
            }
            return 0;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
