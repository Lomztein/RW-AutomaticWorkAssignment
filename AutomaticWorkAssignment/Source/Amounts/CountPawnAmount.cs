using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class CountPawnAmount : PawnAmount
    {
        public List<IPawnCondition> PawnConditions = new List<IPawnCondition>();
        public float Multiplier = 1f;
        private readonly Cache<int> _cache = new Cache<int>();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (_cache.TryGet(out int value))
                return value;

            var pawns = req.WorkManager.GetAllEverAssignablePawns();
            value = (int)(pawns.Count(x => PawnConditions.All(c => c.IsValid(x, spec, req))) * Multiplier);
            return _cache.Set(value);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Multiplier, "multiplier", 1f);
            Scribe_Collections.Look(ref PawnConditions, "conditions");

            PawnConditions ??= new List<IPawnCondition>();
            PawnConditions = PawnConditions.Where(x => x != null && x.IsValidAfterLoad()).ToList();
        }
    }
}
