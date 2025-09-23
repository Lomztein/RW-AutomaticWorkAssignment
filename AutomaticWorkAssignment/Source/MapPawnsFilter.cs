using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source
{
    public class MapPawnsFilter : IExposable
    {
        // Ever available checks
        public bool IncludeColonists = true;
        public bool IncludeGuests = true;
        public bool IncludeSlaves = true;
        public bool IncludePrisoners = false;

        // Temp available checks
        public bool IncludeDowned = false;
        public bool IncludeMentallyBroken = false;
        public bool IncludeCryptosleepers = false;

        public List<PawnRef> ExcludedPawns = new List<PawnRef>();

        public static float MaxMentalBreakHours => AutomaticWorkAssignmentSettings.MentalBreakHourThreshold;

        public void ExposeData()
        {
            Scribe_Values.Look(ref IncludeColonists, "includeColonists", true);
            Scribe_Values.Look(ref IncludeGuests, "includeGuests", true);
            Scribe_Values.Look(ref IncludeSlaves, "includeSlaves", true);
            Scribe_Values.Look(ref IncludePrisoners, "includePriosoners", false);

            Scribe_Values.Look(ref IncludeDowned, "includeDowned", false);
            Scribe_Values.Look(ref IncludeMentallyBroken, "includeMentallyBroken", false);
            Scribe_Values.Look(ref IncludeCryptosleepers, "includeCryptosleepers", false);
            Scribe_Collections.Look(ref ExcludedPawns, "excludedPawns");


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ExcludedPawns ??= new List<PawnRef>();
            }
        }

        public IEnumerable<Pawn> GetEverAvailablePawns(IEnumerable<Pawn> allPawns, Map map)
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

        public IEnumerable<Pawn> RemoveTemporarilyUnavailablePawns(IEnumerable<Pawn> allPawns)
        {
            foreach (Pawn pawn in allPawns)
            {
                bool include = true;

                if (pawn.DeadOrDowned && !IncludeDowned)
                    include = false;
                if (IsMentalStateBlocking(pawn) && !IncludeMentallyBroken)
                    include = false;
                if (pawn.InCryptosleep && !IncludeCryptosleepers)
                    include = false;

                if (include)
                    yield return pawn;
            }
        }

        private static bool IsMentalStateBlocking(Pawn pawn)
        {
            if (pawn != null && pawn.MentalStateDef != null)
            {
                if (pawn.MentalStateDef.maxTicksBeforeRecovery > GenDate.TicksPerHour * MaxMentalBreakHours) return true;
                if (pawn.MentalStateDef.IsExtreme) return true;
                if (pawn.MentalStateDef.IsAggro) return true;
            }
            return false;
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
