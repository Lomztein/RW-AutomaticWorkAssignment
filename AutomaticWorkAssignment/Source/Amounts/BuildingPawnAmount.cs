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

        public int GetCount()
        {
            if (BuildingDef != null)
            {
                return Mathf.RoundToInt(WorkManager.Instance.GetAllMaps()
                    .SelectMany(x => x.listerBuildings.allBuildingsColonist)
                    .Count(x => x.def == BuildingDef) * Multiplier);
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
