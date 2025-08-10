using BetterPawnControl;
using HarmonyLib;
using Lomzie.AutomaticWorkAssignment;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.BetterPawnControl
{
    [StaticConstructorOnStartup]
    public class BetterPawnControlPatch
    {
        private static readonly Harmony _harmony;

        public static string AutoWorkPolicy = "Default";
        public static string AutoSchedulePolicy = "Default";
        public static string AutoAssignPolicy = "Default";

        private static MethodInfo _workManager_getActivePolicy;
        private static MethodInfo _workManager_saveCurrentState;

        private static MethodInfo _scheduleManager_getActivePolicy;
        private static MethodInfo _scheduleManager_saveCurrentState;

        private static MethodInfo _assignManager_getActivePolicy;
        private static MethodInfo _assignManager_saveCurrentState;

        static BetterPawnControlPatch()
        {
            _harmony = new Harmony("Lomzie.AutomaticWorkAssignment.BetterPawnControl");
            Do(_harmony);
        }

        public static void Do(Harmony harm)
        {
            _workManager_getActivePolicy = AccessTools.Method("BetterPawnControl.WorkManager:GetActivePolicy", new[] { typeof(int) });
            _workManager_saveCurrentState = AccessTools.Method("BetterPawnControl.WorkManager:SaveCurrentState", new[] { typeof(List<Pawn>) });

            _scheduleManager_getActivePolicy = AccessTools.Method("BetterPawnControl.ScheduleManager:GetActivePolicy", new[] { typeof(int) });
            _scheduleManager_saveCurrentState = AccessTools.Method("BetterPawnControl.ScheduleManager:SaveCurrentState", new[] { typeof(List<Pawn>) });

            _assignManager_getActivePolicy = AccessTools.Method("BetterPawnControl.AssignManager:GetActivePolicy", new[] { typeof(int) });
            _assignManager_saveCurrentState = AccessTools.Method("BetterPawnControl.AssignManager:SaveCurrentState", new[] { typeof(List<Pawn>) });

            MethodInfo resolveMethod = AccessTools.Method(typeof(MapWorkManager), "ResolvePriorities", new[] { typeof(ResolveWorkRequest) });
            harm.Patch(resolveMethod, postfix: new HarmonyMethod(new Action<ResolveWorkRequest>(WorkManager_ResolvePriorities_PostFix)));

            MethodInfo settings_doListing = AccessTools.Method(typeof(AutomaticWorkAssignmentSettings), "DoListing", new[] { typeof(Listing_Standard) });
            harm.Patch(settings_doListing, postfix: new Action<Listing_Standard>(Settings_DoListing_PostFix));

            MethodInfo settings_exposeData = AccessTools.Method(typeof(AutomaticWorkAssignmentSettings), "ExposeData");
            harm.Patch(settings_doListing, postfix: new Action<Listing_Standard>(Settings_ExposeData_PostFix));

            Log.Message("[AWA] Applied BetterPawnControl patch.");
        }

        private static void Settings_ExposeData_PostFix(Listing_Standard listing)
        {
            Scribe_Values.Look(ref AutoWorkPolicy, "autoWorkPolicy");
            Scribe_Values.Look(ref AutoSchedulePolicy, "autoSchedulePolicy");
            Scribe_Values.Look(ref AutoAssignPolicy, "autoAssignPolicy");
        }

        private static void Settings_DoListing_PostFix(Listing_Standard listing)
        {
            listing.GapLine();
            listing.Label("AWA.BPC.SettingsHeader".Translate());
            AutoWorkPolicy = listing.TextEntryLabeled("AWA.BPC.SettingsAutoWorkPolicy".Translate(), AutoWorkPolicy);
            AutoSchedulePolicy = listing.TextEntryLabeled("AWA.BPC.SettingsAutoSchedulePolicy".Translate(), AutoSchedulePolicy);
            AutoAssignPolicy = listing.TextEntryLabeled("AWA.BPC.SettingsAutoAssignPolicy".Translate(), AutoAssignPolicy);
        }

        private static void WorkManager_ResolvePriorities_PostFix(ResolveWorkRequest req)
        {
            Policy active;

            // Save work priorities.
            active = _workManager_getActivePolicy.Invoke(null, new object[] { req.Map.uniqueID }) as Policy;
            if (active.label == AutoWorkPolicy)
            {
                _workManager_saveCurrentState.Invoke(null, new[] { req.Pawns });
            }

            // Save schedule policy.
            active = _scheduleManager_getActivePolicy.Invoke(null, new object[] { req.Map.uniqueID }) as Policy;
            if (active.label == AutoSchedulePolicy)
            {
                _scheduleManager_saveCurrentState.Invoke(null, new[] { req.Pawns });
            }

            // Save assign policy.
            active = _assignManager_getActivePolicy.Invoke(null, new object[] { req.Map.uniqueID }) as Policy;
            if (active.label == AutoAssignPolicy)
            {
                _assignManager_saveCurrentState.Invoke(null, new[] { req.Pawns });
            }
        }
    }
}
