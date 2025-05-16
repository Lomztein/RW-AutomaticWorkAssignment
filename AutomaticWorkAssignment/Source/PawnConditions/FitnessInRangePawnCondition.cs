using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class FitnessInRangePawnCondition : PawnSetting, IPawnCondition
    {
        public IPawnFitness Fitness;
        public float Min, Max;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (Fitness != null)
            {
                float fitness = Fitness.CalcFitness(pawn, specification, request);
                return fitness >= Min && fitness <= Max;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Fitness, "fitness");
            Scribe_Values.Look(ref Min, "min");
            Scribe_Values.Look(ref Max, "max");
        }
    }
}
