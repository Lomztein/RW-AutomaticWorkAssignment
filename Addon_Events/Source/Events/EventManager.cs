using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events
{
    public class EventManager : WorldComponent
    {
        private List<PawnEventManager> _pawnEventManagers = new List<PawnEventManager>();
        private readonly Dictionary<Pawn, PawnEventManager> _cache = new Dictionary<Pawn, PawnEventManager>();

        private static EventManager _instance;

        public EventManager(World world) : base(world)
        {
            _instance = this;
        }

        public static void Reset()
        {
            foreach (PawnEventManager pawnEventManager in _instance._pawnEventManagers)
            {
                pawnEventManager.Reset();
            }
        }

        public static void Subscribe(Pawn pawn, EventDef eventDef, WorkSpecification workSpec, IPawnPostProcessor postProcessor)
        {
            PawnEventManager eventManager = _instance.GetPawnEventManager(pawn);
            if (eventManager == null)
            {
                eventManager = new PawnEventManager(pawn);
                _instance._pawnEventManagers.Add(eventManager);
            }
            eventManager.Subscribe(eventDef, workSpec, postProcessor);
        }

        public static void InvokePawnEvent(Pawn pawn, EventDef eventDef)
        {
            if (pawn.Map != null)
            {
                ResolveWorkRequest req = MapWorkManager.GetManager(pawn.Map).MakeDefaultRequest();
                _instance.GetPawnEventManager(pawn)?.Invoke(eventDef, req);
            }
        }

        public static void InvokeMapEvent(Map map, EventDef eventDef)
        {
            ResolveWorkRequest req = MapWorkManager.GetManager(map).MakeDefaultRequest();
            var allPawns = map.mapPawns.AllHumanlikeSpawned;
            foreach (var pawn in allPawns)
            {
                PawnEventManager pawnManager = _instance.GetPawnEventManager(pawn);
                pawnManager.Invoke(eventDef, req);
            }
        }

        public static void InvokeWorldEvent(World world, EventDef eventDef)
        {
            // TODO: Delete me if I never end up used.
        }

        public PawnEventManager GetPawnEventManager(Pawn pawn)
        {
            if (!_cache.TryGetValue(pawn, out var pawnEventManager))
            {
                pawnEventManager = _pawnEventManagers.FirstOrDefault(x => x.Pawn == pawn);
                if (pawnEventManager == null)
                    return null;
                _cache.Add(pawn, pawnEventManager);
            }
            return pawnEventManager;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _pawnEventManagers, "pawnEventManagers", LookMode.Deep);
        }
    }
}
