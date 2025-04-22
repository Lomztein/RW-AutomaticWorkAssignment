using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class PassionCountPawnFitness : IPawnFitness
    {
        public string Label => "Passion count";
        public string Description => "Count the amount of passions a pawn has.";

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                return pawn?.skills?.PassionCount ?? 0;
            }
            return 0f;
        }

        public void ExposeData()
        {
        }
    }
}
