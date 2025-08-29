using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class PercentagePawnAmount : PawnAmount
    {
        public float Percentage;
        private readonly Cache<int> _cache = new();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Percentage, "percentage");
        }

        public override int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (_cache.TryGet(out int value))
                return value;

            int colonistCount = request.WorkManager.GetAllAssignableNowPawns().Count();
            value = Mathf.RoundToInt(colonistCount * Percentage);

            return _cache.Set(value);
        }
    }
}
