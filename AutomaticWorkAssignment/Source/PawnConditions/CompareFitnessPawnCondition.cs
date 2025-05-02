using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class CompareFitnessPawnCondition : CompositePawnSetting, IPawnCondition
    {
        public override int MaxSettings => 2;

        public enum Comparsion
        {
            Equals, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual
        }

        public Comparsion ComparisonType;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count == 2)
            {
                IPawnFitness lhs = InnerSettings[0] as IPawnFitness;
                IPawnFitness rhs = InnerSettings[1] as IPawnFitness;
                return DoComparison(lhs.CalcFitness(pawn, specification, request), rhs.CalcFitness(pawn, specification, request));
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ComparisonType, "comparisonType");
        }

        private bool DoComparison(float v1, float v2)
        {
            switch (ComparisonType)
            {
                case Comparsion.Equals:
                    return v1 == v2;
                case Comparsion.NotEquals:
                    return v1 != v2;
                case Comparsion.LessThan:
                    return v1 < v2;
                case Comparsion.LessThanOrEqual:
                    return v1 <= v2;
                case Comparsion.GreaterThan:
                    return v1 > v2;
                case Comparsion.GreaterThanOrEqual:
                    return v1 >= v2;
                default: 
                    throw new InvalidOperationException($"Unsupported comparison type {ComparisonType}");
            }
        }
    }
}
