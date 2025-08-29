using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class FarmlandPawnAmount : PawnAmount
    {
        public float Multiplier = 0.01f;

        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt((req.WorkManager.GetAllMaps()
                    .SelectMany(x => x.zoneManager.AllZones).Where(x => x is Zone_Growing)
                    .Sum(x => x.CellCount) + 
                    req.Map.listerBuildings.allBuildingsColonist
                    .Where(x => x is IPlantToGrowSettable)
                    .Sum(x => (x as IPlantToGrowSettable).Cells.Count())) * Multiplier);

            return _cache.Set(value);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
