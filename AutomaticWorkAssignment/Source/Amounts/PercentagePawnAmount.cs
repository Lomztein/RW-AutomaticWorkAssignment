using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class PercentagePawnAmount : IPawnAmount
    {
        public float Percentage;
        private Cache<int> _cache;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Percentage, "percentage");
        }

        public int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (_cache == null)
            {
                _cache = new Cache<int>(() =>
                {
                    int colonistCount = request.WorkManager.GetAllAssignableNowPawns().Count();
                    return Mathf.RoundToInt(colonistCount * Percentage);
                });
            }

            return _cache.Get();
        }
    }
}
