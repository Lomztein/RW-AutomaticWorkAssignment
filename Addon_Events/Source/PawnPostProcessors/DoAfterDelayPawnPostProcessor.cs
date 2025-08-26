using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DoAfterDelayPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public float DelayHours;
        private float TicksPerTimeUnit => GenDate.TicksPerHour;
        public IPawnPostProcessor Action;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Action != null)
            {
                Find.Root.StartCoroutine(ExecuteAfterDelay(() => Action.PostProcess(pawn, workSpecification, request)));
            }
        }

        private IEnumerator ExecuteAfterDelay(Action action)
        {
            float delayTicks = DelayHours * TicksPerTimeUnit;
            float targetTime = GenTicks.TicksAbs + delayTicks;
            while (GenTicks.TicksAbs < targetTime)
            {
                yield return null;
            }
            action();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DelayHours, "delayHours");
            Scribe_Deep.Look(ref Action, "action");
        }
    }
}
