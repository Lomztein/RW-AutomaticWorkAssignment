using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class GuestsPawnAmount : PawnAmount
    {
        public float Multiplier = 0.25f;

        private readonly Cache<int> _cache = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            value = Mathf.CeilToInt(req.WorkManager.GetAllMaps().SelectMany(x => x.mapPawns.PrisonersOfColony).Count(x => IsGuest(x, req)));
            return _cache.Set(value);
        }

        private bool IsGuest(Pawn pawn, ResolveWorkRequest request)
        {
            if (request.Map.ParentFaction != null)
            {
                return pawn.HomeFaction != request.Map.ParentFaction;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier");
        }
    }
}
