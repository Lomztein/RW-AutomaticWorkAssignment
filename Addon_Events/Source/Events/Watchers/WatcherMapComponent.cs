using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events.Watchers
{
    public class WatcherMapComponent : MapComponent
    {
        private readonly List<Watcher<WatcherMapComponent>> _watchers;

        public WatcherMapComponent(Map map) : base(map)
        {
            _watchers = Watcher<WatcherMapComponent>.GetAllWatchers().ToList();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach (var watcher in _watchers)
            {
                if (Gen.IsHashIntervalTick(map, watcher.TickInterval))
                {
                    watcher.Tick(this);
                }
            }
        }
    }
}
