using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public abstract class AggregatePawnFitness : CompositePawnSetting, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count > 0)
                return InnerSettings.Select(x => (x as IPawnFitness).CalcFitness(pawn, specification, request)).Aggregate(Aggregate);
            return 0f;
        }

        public abstract float Aggregate(float accumulate, float source);
    }

    public class SumPawnFitness : AggregatePawnFitness
    {
        public override string Label => "Sum";
        public override string Description => "Sum of nested functions.";

        public override float Aggregate(float accumulate, float source)
        {
            accumulate += source;
            return accumulate;
        }
    }

    public class ProductPawnFitness : AggregatePawnFitness
    {
        public override string Label => "Multiply";
        public override string Description => "Multiply nested functions.";

        public override float Aggregate(float accumulate, float source)
        {
            accumulate *= source;
            return accumulate;
        }
    }

    public class MinPawnFitness : AggregatePawnFitness
    {
        public override string Label => "Min";
        public override string Description => "Min of nested functions.";

        public override float Aggregate(float accumulate, float source)
            => Mathf.Min(accumulate, source);
    }

    public class MaxPawnFitness : AggregatePawnFitness
    {
        public override string Label => "Max";
        public override string Description => "Max of nested functions.";

        public override float Aggregate(float accumulate, float source)
            => Mathf.Max(accumulate, source);
    }

    public class AveragePawnFitness : CompositePawnSetting, IPawnFitness
    {
        public override string Label => "Average";
        public override string Description => "Average of nested functions.";

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count > 0)
                return InnerSettings.Select(x => (x as IPawnFitness).CalcFitness(pawn, specification, request)).Average();
            return 0f;
        }
    }

    public class CountPawnFitness : CompositePawnSetting, IPawnFitness
    {
        public override string Label => "Count";

        public override string Description => "Count the passed nested conditions.";

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return InnerSettings.Select(x => (x as IPawnCondition).IsValid(pawn, specification, request)).Count(x => x == true);
        }
    }
}
