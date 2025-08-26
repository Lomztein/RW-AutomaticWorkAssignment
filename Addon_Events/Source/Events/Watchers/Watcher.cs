using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events.Watchers
{
    public abstract class Watcher<TWatcherComponent>
    {
        public virtual int TickInterval => GenTicks.TickLongInterval;

        public abstract void Tick(TWatcherComponent watcherComponent);

        public static IEnumerable<Watcher<TWatcherComponent>> GetAllWatchers()
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.DefinedTypes).
                Where(x => typeof(Watcher<TWatcherComponent>).IsAssignableFrom(x) && !x.IsAbstract).Select(x => x.AsType());

            foreach (var type in types)
            {
                yield return (Watcher<TWatcherComponent>)Activator.CreateInstance(type);
            }
        }
    }
}
