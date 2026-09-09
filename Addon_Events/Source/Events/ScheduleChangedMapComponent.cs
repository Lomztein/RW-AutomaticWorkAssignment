using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events
{
    // One clock check per map tick; pawn schedules are read only at hour changes.
    public class ScheduleChangedMapComponent : MapComponent
    {
        private int _lastHour = -1;
        private readonly Dictionary<Pawn, TimeAssignmentDef> _assignments = new();
        private readonly HashSet<Pawn> _dispatching = new();

        public ScheduleChangedMapComponent(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            int hour = GenLocalDate.HourInteger(map);
            if (hour == _lastHour) return;
            _lastHour = hour;

            // Snapshot: event actions may change the map's pawn list.
            var pawns = new List<Pawn>(map.mapPawns.FreeColonistsSpawned);
            var present = new HashSet<Pawn>(pawns);
            foreach (var pawn in new List<Pawn>(_assignments.Keys))
                if (!present.Contains(pawn)) _assignments.Remove(pawn);

            foreach (var pawn in pawns)
            {
                if (pawn.Map != map || pawn.timetable == null) continue;
                TimeAssignmentDef current = pawn.timetable.CurrentAssignment;
                if (_assignments.TryGetValue(pawn, out var previous))
                    NotifyChanged(pawn, previous, current);
                else
                    _assignments[pawn] = current; // Load/arrival is not a schedule change.
            }
        }

        public void NotifyChanged(Pawn pawn, TimeAssignmentDef previous, TimeAssignmentDef current)
        {
            _assignments[pawn] = current;
            if (previous == current || !_dispatching.Add(pawn)) return;
            try
            {
                EventManager.InvokePawnEvent(pawn, EventDefOf.ScheduleChanged);
            }
            finally
            {
                _dispatching.Remove(pawn);
            }
        }
    }
}
