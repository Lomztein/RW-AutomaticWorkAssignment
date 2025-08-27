using HarmonyLib;
using Lomzie.AutomaticWorkAssignment.UI.Dialogs;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
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

            var mainMenuInit = AccessTools.Method("UIRoot_Entry:Init");
            harmony.Patch(mainMenuInit, postfix: new Action(CheckDependencies));
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

        private static void CheckDependencies()
        {
            if (!IsModActive("kathanon.floatsubmenu"))
            {
                Find.WindowStack.Add(new Dialog_BigConfirm("AWA.Warning.FloatSubMenusNotActive".Translate(), () => Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2864015430"), new Vector2(500, 170)));
            }
        }

        private static bool IsModActive(string modId)
            => ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId == modId);
    }
}
