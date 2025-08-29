using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class StockpilePawnAmount : PawnAmount
    {
        public ThingFilter ThingFilter = new();
        public float Multiplier = 0.1f;

        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ThingFilter, "thingFilter");
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
