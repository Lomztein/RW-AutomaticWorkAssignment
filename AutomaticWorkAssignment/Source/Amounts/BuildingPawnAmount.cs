using Lomzie.AutomaticWorkAssignment.Amounts;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class BuildingPawnAmount : IPawnAmount
    {
        public ThingDef BuildingDef;
        public float Multiplier = 1f;

        private Cache<int> _cache;

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache == null)
            {
                _cache = new Cache<int>(() => Mathf.RoundToInt(req.WorkManager.GetAllMaps()
                    .SelectMany(x => x.listerBuildings.allBuildingsColonist)
                    .Count(x => x.def == BuildingDef) * Multiplier));
            }

            if (BuildingDef != null)
            {
                return _cache.Get();
            }
            return 0;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref BuildingDef, "buildingDef");
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
