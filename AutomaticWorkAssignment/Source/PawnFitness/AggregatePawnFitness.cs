﻿using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public abstract class AggregatePawnFitness : CompositePawnSetting<IPawnFitness>, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count > 0)
                return InnerSettings.Select(x => x.CalcFitness(pawn, specification, request)).Aggregate(Aggregate);
            return 0f;
        }

        public abstract float Aggregate(float accumulate, float source);
    }

    public class SumPawnFitness : AggregatePawnFitness
    {
        public override float Aggregate(float accumulate, float source)
        {
            accumulate += source;
            return accumulate;
        }
    }

    public class ProductPawnFitness : AggregatePawnFitness
    {
        public override float Aggregate(float accumulate, float source)
        {
            accumulate *= source;
            return accumulate;
        }
    }

    public class MinPawnFitness : AggregatePawnFitness
    {
        public override float Aggregate(float accumulate, float source)
            => Mathf.Min(accumulate, source);
    }

    public class MaxPawnFitness : AggregatePawnFitness
    {
        public override float Aggregate(float accumulate, float source)
            => Mathf.Max(accumulate, source);
    }

    public class AveragePawnFitness : CompositePawnSetting<IPawnFitness>, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count > 0)
                return InnerSettings.Select(x => x.CalcFitness(pawn, specification, request)).Average();
            return 0f;
        }
    }

    public class CountPawnFitness : CompositePawnSetting<IPawnCondition>, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return InnerSettings.Select(x => x.IsValid(pawn, specification, request)).Count(x => x == true);
        }
    }

    public class CountPawnsPawnFitness : CompositePawnSetting<IPawnCondition>, IPawnFitness
    {
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (InnerSettings.Count > 0)
                return InnerSettings.Sum(x => request.Pawns.Count(y => x.IsValid(y, specification, request)));
            return request.Pawns.Count;
        }
    }
}
