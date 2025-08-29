using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class AnimalsPawnAmount : PawnAmount
    {
        public float Multiplier = 0.1f;
        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps()
                    .SelectMany(x => x.mapPawns.AllPawnsSpawned).Where(x => x.IsAnimal && x.Faction == Faction.OfPlayer)
                    .Count() * Multiplier);
            _cache.Set(value);
            return value;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
