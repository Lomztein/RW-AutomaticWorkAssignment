using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ConstantPawnFitness : PawnSetting, IPawnFitness
    {
        public float Value;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return Value;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Value, "value");
        }
    }
}
