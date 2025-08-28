using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class AnimalsPawnAmount : IPawnAmount
    {
        public float Multiplier = 0.1f;

        private Cache<int> _cache = new Cache<int>();

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            _cache = new Cache<int>();
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps()
                    .SelectMany(x => x.mapPawns.AllPawnsSpawned).Where(x => x.IsAnimal && x.Faction == Faction.OfPlayer)
                    .Count() * Multiplier);
            _cache.Set(value);
            return value;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
