using HarmonyLib;
using Lomzie.AutomaticWorkAssignment.Events;
using Lomzie.AutomaticWorkAssignment.Events.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source.Events.Hooks
{
    internal class HealthConditionAddedHook : EventHook
    {
        private static FieldInfo _pawnHealthTracker_pawn;

        public override void DoHook(Harmony harm)
        {
            MethodInfo addHediff = AccessTools.Method("Pawn_HealthTracker:AddHediff", new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) });
            _pawnHealthTracker_pawn = AccessTools.Field("Pawn_HealthTracker:pawn");
            harm.Patch(addHediff, postfix: new Action<Pawn_HealthTracker>(AddHediffHook));
        }

        private static void AddHediffHook(Pawn_HealthTracker __instance)
        {
            Pawn pawn = _pawnHealthTracker_pawn.GetValue(__instance) as Pawn;
            EventManager.InvokePawnEvent(pawn, EventDefOf.HealthConditionAdded);
        }
    }
}
