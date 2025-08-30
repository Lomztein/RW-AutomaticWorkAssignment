using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source
{
    public class MapPawnsFilter : IExposable
    {
        public bool IncludeColonists = true;
        public bool IncludeGuests = true;
        public bool IncludeSlaves = true;
        public bool IncludePrisoners = false;
        public List<PawnRef> ExcludedPawns = new List<PawnRef>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref IncludeColonists, "includeColonists", true);
            Scribe_Values.Look(ref IncludeGuests, "includeGuests", true);
            Scribe_Values.Look(ref IncludeSlaves, "includeSlaves", true);
            Scribe_Values.Look(ref IncludePrisoners, "includePriosoners", false);
            Scribe_Collections.Look(ref ExcludedPawns, "excludedPawns");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ExcludedPawns ??= new List<PawnRef>();
            }
        }

        public IEnumerable<Pawn> FilterPawns(IEnumerable<Pawn> allPawns, Map map)
        {
            foreach (Pawn pawn in allPawns)
            {
                if (ExcludedPawns.Any(x => x.Is(pawn)))
                    continue;

                if (pawn.IsPrisonerOfColony)
                {
                    if (IncludePrisoners)
                        yield return pawn;
                    continue;
                }

                if (pawn.IsSlaveOfColony)
                {
                    if (IncludeSlaves)
                        yield return pawn;
                    continue;
                }

                if (IsGuest(pawn, map))
                {
                    if (IncludeGuests)
                        yield return pawn;
                    continue;
                }

                if (pawn.IsColonist)
                {
                    if (IncludeColonists)
                        yield return pawn;
                    continue;
                }
            }
        }

        private bool IsGuest(Pawn pawn, Map map)
        {
            if (!pawn.IsColonist)
                return false;

            if (map.ParentFaction != null)
            {
                return pawn.HomeFaction != map.ParentFaction;
            }
            return false;
        }
    }
}
