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
        public WorkSpecification Specification { get; set; }
        public Pawn Pawn { get; set; }

        public WorkAssignment (WorkSpecification specification, Pawn pawn)
        {
            Specification = specification;
            Pawn = pawn;
        }
    }
}
