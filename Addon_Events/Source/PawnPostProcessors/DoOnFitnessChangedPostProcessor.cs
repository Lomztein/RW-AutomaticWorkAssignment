using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System.Collections;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DoOnFitnessChangedPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public IPawnFitness Fitness;
        public float Threshold;
        public IPawnPostProcessor Action;

        private float _value;

        private readonly Buffer<Coroutine> _buffer = new Buffer<Coroutine>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Action != null && Fitness != null)
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
            _value = Fitness.CalcFitness(pawn, workSpec, request);
            while (true)
            {
                float newValue = Fitness.CalcFitness(pawn, workSpec, request);
                if (Mathf.Abs(_value - newValue) >= Threshold)
                {
                    _value = newValue;
                    Action.PostProcess(pawn, workSpec, request);
                }
                yield return new WaitForSeconds(1);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Fitness, "fitness");
            Scribe_Values.Look(ref Threshold, "threshold");
            Scribe_Deep.Look(ref Action, "action");
        }

        ~DoOnFitnessChangedPawnPostProcessor()
        {
            foreach (var value in _buffer.GetValues())
            {
                if (Find.Root != null)
                {
                    Find.Root.StopCoroutine(value);
                }
            }
        }
    }
}
