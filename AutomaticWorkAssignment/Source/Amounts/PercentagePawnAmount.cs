using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class PercentagePawnAmount : IPawnAmount
    {
        public float Percentage;
        private Cache<int> _cache = new Cache<int>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref Percentage, "percentage");
        }

        public int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (_cache.TryGet(out int value))
                return value;

            int colonistCount = request.WorkManager.GetAllAssignableNowPawns().Count();
            value = Mathf.RoundToInt(colonistCount * Percentage);

            return _cache.Set(value);
        }
    }
}
