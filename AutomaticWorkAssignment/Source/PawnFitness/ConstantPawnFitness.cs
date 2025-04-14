using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ConstantPawnFitness : IPawnFitness
    {
        public string Label => "Constant";
        public string Description => "Constant value, intended for use in comparisons.";

        public float Value;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return Value;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Value, "value");
        }
    }
}
