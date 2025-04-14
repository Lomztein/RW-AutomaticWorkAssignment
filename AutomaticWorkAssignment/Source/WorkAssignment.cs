using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkAssignment
    {
        public WorkSpecification Specification;
        public Pawn Pawn;

        public int Index;
        public bool IsCritical;

        public WorkAssignment Substitution;
        public bool IsSubstituted => Substitution != null;

        public WorkAssignment (WorkSpecification specification, Pawn pawn, int index, bool isCritical)
        {
            Specification = specification;
            Pawn = pawn;
            Index = index;
            IsCritical = isCritical;
        }

        public void SubstituteWith(WorkAssignment substitution)
            => Substitution = substitution;
    }
}
