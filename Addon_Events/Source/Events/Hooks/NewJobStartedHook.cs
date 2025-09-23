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
using Verse.AI;

namespace Lomzie.AutomaticWorkAssignment.Source.Events.Hooks
{
    internal class NewJobStartedHook : EventHook
    {
        private static FieldInfo _pawnJobTracker_pawn;

        public override void DoHook(Harmony harm)
        {
            MethodInfo newJobStarted = AccessTools.Method("Pawn_JobTracker:StartJob");
            _pawnJobTracker_pawn = AccessTools.Field("Pawn_JobTracker:pawn");
            harm.Patch(newJobStarted, postfix: new Action<Pawn_JobTracker>(StartJobHook));
        }

        private static void StartJobHook(Pawn_JobTracker __instance)
        {
            Pawn pawn = _pawnJobTracker_pawn.GetValue(__instance) as Pawn;
            if (pawn.IsColonist)
            {
                EventManager.InvokePawnEvent(pawn, EventDefOf.NewJobStarted);
            }
        }
    }
}
