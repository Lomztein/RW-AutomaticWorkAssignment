using Lomzie.AutomaticWorkAssignment.PawnConditions;
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
    public class DoOnConditionChangedPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public IPawnCondition Condition;
        public IPawnPostProcessor Action;

        private readonly Buffer<Coroutine> _buffer = new Buffer<Coroutine>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Condition != null && Action != null)
            {
                Coroutine current = _buffer.Get(pawn);
                if (current != null)
                    Find.Root.StopCoroutine(current);

                Coroutine coroutine = Find.Root.StartCoroutine(Check(pawn, workSpecification, request));
                _buffer.Set(pawn, coroutine);
            }
        }

        private IEnumerator Check(Pawn pawn, WorkSpecification workSpec, ResolveWorkRequest request)
        {
            bool value = Condition.IsValid(pawn, workSpec, request);
            while (true)
            {
                bool newValue = Condition.IsValid(pawn, workSpec, request);
                if (newValue != value)
                {
                    value = newValue;
                    Action.PostProcess(pawn, workSpec, request);
                }
                yield return new WaitForSeconds(1);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Condition, "condition");
            Scribe_Deep.Look(ref Action, "action");
        }

        ~DoOnConditionChangedPawnPostProcessor()
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
