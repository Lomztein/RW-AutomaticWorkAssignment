using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomzie.AutomaticWorkAssignment.Events.Hooks
{
    public abstract class EventHook
    {
        public virtual bool Enabled => true;

        public abstract void DoHook(Harmony harm);

        public static IEnumerable<EventHook> GetAllEventHooks ()
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.DefinedTypes).
                Where(x => typeof(EventHook).IsAssignableFrom(x) && !x.IsAbstract).Select(x => x.AsType());

            foreach (var type in types)
            {
                yield return (EventHook)Activator.CreateInstance(type);
            }
        }
    }
}
