using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events
{
    public class PawnEventManager : IExposable
    {
        public Pawn Pawn;
        private List<Event> _events = new();
        private readonly Dictionary<EventDef, Event> _cache = new Dictionary<EventDef, Event>();

        public PawnEventManager() { }

        public PawnEventManager(Pawn pawn)
        {
            Pawn = pawn;
        }

        public void Subscribe(EventDef toEvent, WorkSpecification workSpec, IPawnPostProcessor postProcessor)
        {
            Event pawnEvent = GetEvent(toEvent);
            if (pawnEvent == null)
            {
                pawnEvent = Event.Create(toEvent);
                _events.Add(pawnEvent);
            }
            pawnEvent.Subscribe(workSpec, postProcessor);
        }

        public void Invoke(EventDef eventDef, ResolveWorkRequest resolveWorkRequest)
        {
            Event toInvoke = GetEvent(eventDef);
            if (toInvoke != null)
            {
                toInvoke.Invoke(Pawn, resolveWorkRequest);
            }
        }

        private Event GetEvent(EventDef def)
        {
            if (!_cache.TryGetValue(def, out Event pawnEvent))
            {
                pawnEvent = _events.FirstOrDefault(x => x.Def == def);
                if (pawnEvent == null)
                    return null;
                _cache.Add(def, pawnEvent);
            }
            return pawnEvent;
        }

        public void Reset()
        {
            foreach (var pawnEvent in _events)
            {
                pawnEvent.Reset();
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "pawn");
            Scribe_Collections.Look(ref _events, "events");

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                _events.RemoveAll(x => !x.AnySubscribers());
            }
        }
    }
}
