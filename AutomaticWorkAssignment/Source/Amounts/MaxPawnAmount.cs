using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class MaxPawnAmount : IPawnAmount
    {
        public List<IPawnAmount> InnerAmounts = new List<IPawnAmount>();

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (InnerAmounts.Count == 0) return 0;
            return InnerAmounts.Max(x => x.GetCount(spec, req));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref InnerAmounts, "innerAmounts", LookMode.Deep);
        }
    }
}
