using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SpecificPawnCondition : PawnSetting, IPawnCondition
    {
        public Pawn Pawn;
        private string _uniqueId;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Pawn, "pawn");

            if (Scribe.mode == LoadSaveMode.Saving)
                _uniqueId = Pawn?.GetUniqueLoadID();

            Scribe_Values.Look(ref _uniqueId, "uniqueId");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Pawn == null && _uniqueId != null)
                Pawn = Find.World.PlayerPawnsForStoryteller.FirstOrDefault(x => x.GetUniqueLoadID() == _uniqueId);
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn == Pawn;
    }
}
