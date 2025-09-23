using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class PawnRef : IExposable
    {
        public Pawn Pawn;
        private string _uniqueId;

        public PawnRef() { }

        public PawnRef(Pawn pawn)
        {
            Pawn = pawn;
        }

        public bool Is(Pawn pawn)
            => Pawn != null && Pawn == pawn;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "pawn");

            if (Scribe.mode == LoadSaveMode.Saving)
                _uniqueId = Pawn?.GetUniqueLoadID();

            Scribe_Values.Look(ref _uniqueId, "uniqueId");

            if (Scribe.mode != LoadSaveMode.PostLoadInit && Pawn == null && _uniqueId != null)
            {
                Pawn = Find.World.PlayerPawnsForStoryteller.FirstOrDefault(x => x.GetUniqueLoadID() == _uniqueId);
            }
        }
    }
}
