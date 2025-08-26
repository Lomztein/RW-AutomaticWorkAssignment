using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events.Watchers
{
    public class DangerPresentMapWatcher : StateWatcher<WatcherMapComponent>
    {
        public override int TickInterval => GenTicks.TickRareInterval;

        public override bool GetCurrentState(WatcherMapComponent watcherComponent)
            => watcherComponent.map.dangerWatcher.DangerRating != RimWorld.StoryDanger.None;

        public override void OnEventChanged(bool newValue, WatcherMapComponent watcherComponent)
        {
            if (newValue)
                EventManager.InvokeMapEvent(watcherComponent.map, EventDefOf.ThreatAppeared);
            if (!newValue)
                EventManager.InvokeMapEvent(watcherComponent.map, EventDefOf.ThreatCleared);
        }
    }
}
