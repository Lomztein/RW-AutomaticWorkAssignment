using HarmonyLib;
using Lomzie.AutomaticWorkAssignment.Events;
using Lomzie.AutomaticWorkAssignment.Events.Hooks;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source.Events.Hooks
{
    internal class ScheduleChangedHook : EventHook
    {
        public override void DoHook(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Pawn_TimetableTracker), nameof(Pawn_TimetableTracker.SetAssignment)),
                prefix: new HarmonyMethod(typeof(ScheduleChangedHook), nameof(BeforeSetAssignment)),
                postfix: new HarmonyMethod(typeof(ScheduleChangedHook), nameof(AfterSetAssignment)));
        }

        private static void BeforeSetAssignment(Pawn_TimetableTracker __instance, Pawn ___pawn,
            out TimeAssignmentDef __state)
        {
            __state = ___pawn?.Map != null && ___pawn.IsColonist && !___pawn.IsPrisonerOfColony
                ? __instance.CurrentAssignment : null;
        }

        private static void AfterSetAssignment(Pawn_TimetableTracker __instance, Pawn ___pawn,
            TimeAssignmentDef __state)
        {
            if (__state == null || ___pawn?.Map == null) return;
            var current = __instance.CurrentAssignment;
            if (__state != current)
                ___pawn.Map.GetComponent<ScheduleChangedMapComponent>()
                    ?.NotifyChanged(___pawn, __state, current);
        }
    }
}
