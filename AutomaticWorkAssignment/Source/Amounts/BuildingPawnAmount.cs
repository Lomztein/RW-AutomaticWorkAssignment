using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class BuildingPawnAmount : PawnAmount
    {
        public ThingDef BuildingDef;
        public float Multiplier = 1f;

        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (BuildingDef != null)
            {
                if (_cache.TryGet(out int value))
                    return value;

                value = Mathf.RoundToInt(req.WorkManager.GetAllMaps()
                    .SelectMany(x => x.listerBuildings.allBuildingsColonist)
                    .Count(x => x.def == BuildingDef) * Multiplier);

                _cache.Set(value);
                return value;
            }
            return 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref BuildingDef, "buildingDef");
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
