using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class MaxPawnAmount : PawnAmount
    {
        public List<IPawnAmount> InnerAmounts = new();

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            if (InnerAmounts.Count == 0) return 0;
            return InnerAmounts.Max(x => x.GetCount(spec, req));
        }

        public override void ExposeData()
        {
            InnerAmounts.Clear();
            Scribe_Collections.Look(ref InnerAmounts, "innerAmounts", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (InnerAmounts == null) InnerAmounts = new List<IPawnAmount>();
                InnerAmounts = InnerAmounts.Where(x => x != null).ToList();
            }
        }
    }
}
