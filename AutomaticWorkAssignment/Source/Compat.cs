using HarmonyLib;
using Lomzie.AutomaticWorkAssignment.UI.Dialogs;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    // Shared class for minor compatability fixes with various mods, where an entire project would be overkill.
    internal static class Compat
    {
        private static MethodInfo _moreThanCapable_isBadWork;

        public static void DoCompat()
        {
            Harmony harm = new("Lomzie.AutomaticWorkAssignment.Compat");

            // More Than Capable - Basic compatability that just treats work types that MTC considers "bad work" as disabled for the purposes of AWA.
            if (AnyActive("void.morethancapable"))
            {
                _moreThanCapable_isBadWork = AccessTools.Method("MoreThanCapable.MoreThanCapableMod:IsBadWork");
                if (_moreThanCapable_isBadWork == null)
                    Log.Warning("[AWA] More Than Capable patch failed. Method not found.");
                else
                {
                    MethodInfo workTypeIsDisabled = AccessTools.Method(typeof(Utils), nameof(Utils.WorkTypeIsDisabled));
                    harm.Patch(workTypeIsDisabled, postfix: new Func<bool, Pawn, WorkTypeDef, bool>(IsBadWork_Postfix));
                    Log.Message("[AWA] More Than Capable patch applied.");
                }
            }

            // RimWorld Patches
            var initiateTakeoff = AccessTools.Method("WorldComponent_GravshipController:InitiateTakeoff", new[] { typeof(Building_GravEngine), typeof(PlanetTile) });
            harm.Patch(initiateTakeoff, postfix: new Action<Building_GravEngine, PlanetTile>(InitiateTakeoffPostfix));

            var mainMenuInit = AccessTools.Method("UIRoot_Entry:Init");
            harm.Patch(mainMenuInit, postfix: new Action(CheckDependencies));
        }

        private static bool IsBadWork_Postfix(bool __result, Pawn pawn, WorkTypeDef workType)
            => (bool)_moreThanCapable_isBadWork.Invoke(null, new object[] { pawn, workType });

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
            if (!AnyActive("kathanon.floatsubmenu"))
            {
                Find.WindowStack.Add(new Dialog_BigConfirm("AWA.Warning.FloatSubMenusNotActive".Translate(), () => Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2864015430"), new Vector2(500, 170)));
            }
        }

        private static bool AnyActive(params string[] modIds)
            => ModLister.AnyFromListActive(modIds.ToList());
    }
}
