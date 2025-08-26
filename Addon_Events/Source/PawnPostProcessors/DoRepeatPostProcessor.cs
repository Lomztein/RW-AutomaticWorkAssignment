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
    public class DoRepeatPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public float DelayHours;
        private float TicksPerTimeUnit => GenDate.TicksPerHour;
        public IPawnPostProcessor Action;

        private readonly Buffer<Coroutine> _buffer = new Buffer<Coroutine>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Action != null)
            {
                Coroutine current = _buffer.Get(pawn);
                if (current != null)
                    Find.Root.StopCoroutine(current);

                Coroutine coroutine = Find.Root.StartCoroutine(DoRepeat(() => Action.PostProcess(pawn, workSpecification, request)));
                _buffer.Set(pawn, current);
            }
        }

        private IEnumerator DoRepeat(Action action)
        {
            while (true)
            {
                yield return ExecuteAfterDelay(action);
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

        ~DoRepeatPawnPostProcessor()
        {
            foreach(var value in _buffer.GetValues())
            {
                if (Find.Root != null)
                {
                    Find.Root.StopCoroutine(value);
                }
            }
        }
    }
}
