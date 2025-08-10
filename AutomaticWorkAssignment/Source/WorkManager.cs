using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    // Legacy class kept for backwards compatability.
    public class WorkManager : GameComponent
    {
        public List<WorkSpecification> WorkList = new List<WorkSpecification>();
        public List<Pawn> ExcludePawns = new List<Pawn>();

        public bool RefreshEachDay = false;

        public Dictionary<Pawn, List<WorkAssignment>> PawnAssignments = new Dictionary<Pawn, List<WorkAssignment>>();

        public Reservations Reservations = new Reservations();

        private static WorkManager _instance;
        private int _startTicks;

        public WorkManager(Game game)
        {
            _instance = this;
            _startTicks = GenTicks.TicksGame;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick(); // Nothing else seems to work for delaying the clearing. Tried long events and coroutines.
            if (WorkList.Count > 0 && GenTicks.TicksGame > _startTicks + GenTicks.TickLongInterval)
                ClearLegacy();
        }

        private void ClearLegacy()
        {
            WorkList.Clear();
            ExcludePawns.Clear();
            RefreshEachDay = false;
            Log.Message("[AWA] Cleared legacy work manager.");
        }

        public static WorkManager GetLegacyManager()
            => _instance;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref RefreshEachDay, "refreshEachDay", false);
            Scribe_Collections.Look(ref WorkList, "workSpecifications", LookMode.Deep);
            Scribe_Collections.Look(ref ExcludePawns, "excludePawns", LookMode.Reference);
            Scribe_Deep.Look(ref Reservations, "reservations");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (WorkList == null) WorkList = new List<WorkSpecification>();
                if (ExcludePawns == null) ExcludePawns = new List<Pawn>();
                if (Reservations == null) Reservations = new Reservations();
            }
        }
    }
}
