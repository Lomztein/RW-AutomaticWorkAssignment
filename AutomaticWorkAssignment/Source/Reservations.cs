using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Reservations : IExposable
    {
        private Dictionary<Thing, List<ReservationInfo>> _thingReservations = new Dictionary<Thing, List<ReservationInfo>>();

        public Thing FindReservable(ThingDef thingDef, int count, Map onMap)
            => onMap.listerThings.ThingsOfDef(thingDef).Where(x => x.stackCount - Get(x) >= count).FirstOrDefault();

        public void Reserve(Thing thing, int count)
        {
            if (!_thingReservations.ContainsKey(thing))
            {
                _thingReservations.Add(thing, new List<ReservationInfo>());
            }

            int timeout = (int)(GenDate.TicksPerDay * AutomaticWorkAssignmentSettings.ReservationTimeoutDays);
            ReservationInfo info = new ReservationInfo(count, timeout);

            if (_thingReservations[thing] == null)
                _thingReservations[thing] = new List<ReservationInfo>();

            _thingReservations[thing].Add(info);
        }

        public int Get(Thing thing)
        {
            if (_thingReservations.TryGetValue(thing, out List<ReservationInfo> list))
            {
                var expired = list.Where(x => x.IsTimedOut());
                foreach (var reservation in expired)
                {
                    list.Remove(reservation);
                }
                return list.Sum(x => x.Count);
            }
            return 0;
        }

        public void ExposeData()
        {
        }

        private class ReservationInfo : IExposable
        {
            public int Count;
            private int _startTime;
            public int Timeout;

            public ReservationInfo(int count, int timeout)
            {
                Count = count;
                Timeout = timeout;
                _startTime = GenTicks.TicksGame;
            }

            public ReservationInfo() { }

            public bool IsTimedOut()
            {
                int timeout = _startTime + Timeout;
                return GenTicks.TicksGame > timeout;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref Count, "count");
                Scribe_Values.Look(ref _startTime, "startTime");
                Scribe_Values.Look(ref Timeout, "timeout");
            }
        }
    }
}
