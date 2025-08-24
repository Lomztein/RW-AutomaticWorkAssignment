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

namespace Lomzie.AutomaticWorkAssignment.Source.Events
{
    [StaticConstructorOnStartup]
    public static class EventPatches
    {
        private static Harmony _harmony;

        static EventPatches ()
        {
            _harmony = new Harmony("Lomzie.AutomaticWorkAssignment.Events");
            DoPatches(_harmony);
        }

        public static void DoPatches(Harmony harm)
        {
            IEnumerable<EventHook> eventHooks = EventHook.GetAllEventHooks();
            foreach (EventHook hook in eventHooks)
            {
                hook.DoHook(harm);
            }
            Log.Message("[AWA] Patched event hooks.");
        }
    }
}
