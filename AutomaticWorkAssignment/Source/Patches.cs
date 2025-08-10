using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    [StaticConstructorOnStartup]
    public class Patches
    {
        private static readonly Harmony _harmony;

        static Patches()
        {
            _harmony = new Harmony("Lomzie.AutomaticWorkAssignment");
            Do(_harmony);
        }

        private static void Do(Harmony harmony)
        {
            var initiateTakeoff = AccessTools.Method("WorldComponent_GravshipController:InitiateTakeoff", new[] { typeof(Building_GravEngine), typeof(PlanetTile) });
            harmony.Patch(initiateTakeoff, postfix: new Action<Building_GravEngine, PlanetTile>(InitiateTakeoffPostfix));
        }

        private static void InitiateTakeoffPostfix(Building_GravEngine engine, PlanetTile targetTile)
        {
            if (ModsConfig.OdysseyActive)
            {
                string fileName = engine.GetUniqueLoadID();
                IO.ExportToFile(MapWorkManager.GetManager(engine.Map), fileName, IO.GetGravshipConfigDirectory());
                GravshipUtils.GravshipConfigMigrationFileName = fileName;
            }
        }
    }
}
