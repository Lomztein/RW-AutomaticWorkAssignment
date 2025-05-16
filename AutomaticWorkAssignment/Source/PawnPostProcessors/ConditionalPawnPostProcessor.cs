using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class ConditionalPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public IPawnCondition Condition;
        public IPawnPostProcessor PostProcessor;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (Condition != null && Condition.IsValid(pawn, workSpecification, request))
            {
                if (PostProcessor != null)
                {
                    PostProcessor.PostProcess(pawn, workSpecification, request);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Condition, "condition");
            Scribe_Deep.Look(ref PostProcessor, "postProcessor");
        }
    }
}
