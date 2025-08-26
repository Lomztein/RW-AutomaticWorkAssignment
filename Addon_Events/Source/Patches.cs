using HarmonyLib;
using Lomzie.AutomaticWorkAssignment.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    [StaticConstructorOnStartup]
    public static class Patches
    {
        static Patches ()
        {
            Harmony harm = new Harmony("Lomzie.AutomaticWorkAssignment");
            Do(harm);
        }

        private static void Do(Harmony harm)
        {
            // Reset events on work resolution.
            MethodInfo resolveWork = AccessTools.Method("Lomzie.AutomaticWorkAssignment.MapWorkManager:ResolveWorkAssignments");
            harm.Patch(resolveWork, prefix: new Action(ResetEvents));
        }

        private static void ResetEvents()
        {
            EventManager.Reset();
        }
    }
}
