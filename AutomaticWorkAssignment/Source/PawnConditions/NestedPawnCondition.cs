using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public abstract class NestedPawnCondition : IPawnCondition
    {
        public abstract string Label { get; }
        public abstract string Description { get; }

        public IPawnCondition InnerCondition;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref InnerCondition, "innerCondition");
        }

        public abstract bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request);
    }
}
