using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkSpecification : IExposable
    {
        public string Name; // Label for UI.

        public bool IsCritical; // Job will temporarily be reassigned to another pawn if the prior assignee is unable to work.
        public bool IsIncremental; // Job will be assigned one pawn per assignment iteration.
        public float Commitment; // 0 = Occasional, 0.5 = part-time work, 1.0 = full-time work.

        public List<IPawnCondition> Conditions = new List<IPawnCondition>(); // Conditions required for a pawn to be valid. All conditions must be met.
        public List<IPawnFitness> Fitness = new List<IPawnFitness>(); // Used to sort pawns, later elements are used to break earlier ties.
        public List<IPawnPostProcessor> PostProcessors = new List<IPawnPostProcessor>(); // Used to do additional things to pawns assigned to this work.

        public IPawnAmount MinWorkers = new IntPawnAmount(); // Min workers, conditions will be ignored if applicable are below this.
        public IPawnAmount TargetWorkers = new IntPawnAmount(); // Number of workers to be assigned this particular work.

        public PawnWorkPriorities Priorities = PawnWorkPriorities.CreateEmpty(); // The actual work priorities to be assigned.

        public Pawn[] GetApplicablePawnsSorted(IEnumerable<Pawn> allPawns, ResolveWorkRequest request)
        {
            PawnFitnessComparer comparer = new PawnFitnessComparer(Fitness, this, request);

            // Find applicable pawns.
            var applicable = GetApplicablePawns(allPawns, request);
            int _applicableCount = applicable.Count();
            int minCount = MinWorkers.GetCount();
            if (_applicableCount < minCount)
            {
                int missing = minCount - _applicableCount;
                var substitutesSorted = allPawns.Where(x => !applicable.Contains(x)).ToArray();
                Array.Sort(substitutesSorted, comparer);

                Log.Message($"Min threshold not reached: {missing}");

                var toSubstitute = substitutesSorted.ToList().GetRange(0, Mathf.Min(missing, substitutesSorted.Length));
                return Enumerable.Concat(applicable, toSubstitute).ToArray();
            }

            // Sort by fitness.
            var arr = applicable.ToArray();
            Array.Sort(arr, comparer);

            return arr;
        }

        private IEnumerable<Pawn> GetApplicablePawns(IEnumerable<Pawn> allPawns, ResolveWorkRequest request)
        {
            foreach (Pawn pawn in allPawns)
            {
                if (Conditions.All(x => x.IsValid(pawn, this, request))
                    && !pawn.OneOfWorkTypesIsDisabled(Priorities.OrderedPriorities))
                    yield return pawn;
            }
        }

        public int GetTargetWorkers ()
            => Math.Max(TargetWorkers.GetCount(), MinWorkers.GetCount());

        public void ApplyPostProcessing(Pawn pawn, ResolveWorkRequest request)
        {
            foreach (var postProcessor in PostProcessors)
            {
                postProcessor.PostProcess(pawn, this, request);
            }
        }

        public void MoveFitness(IPawnFitness fitness, int movement)
            => Find.Root.StartCoroutine(DelayedMoveFitness(fitness, movement));

        private IEnumerator DelayedMoveFitness(IPawnFitness fitness, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(Fitness, fitness, movement);
        }

        public void DeleteFitness(IPawnFitness fitness)
            => Find.Root.StartCoroutine(DelayedDeleteFitness(fitness));

        private IEnumerator DelayedDeleteFitness(IPawnFitness fitness)
        {
            yield return new WaitForEndOfFrame();
            Fitness.Remove(fitness);
        }

        public void DeleteCondition(IPawnCondition condition)
            => Find.Root.StartCoroutine(DelayedDeleteCondition(condition));

        private IEnumerator DelayedDeleteCondition(IPawnCondition condition)
        {
            yield return new WaitForEndOfFrame();
            Conditions.Remove(condition);
        }

        public void DeletePostProcess(IPawnPostProcessor postProcess)
            => Find.Root.StartCoroutine(DelayedDeletePostProcess(postProcess));

        private IEnumerator DelayedDeletePostProcess(IPawnPostProcessor postProcess)
        {
            yield return new WaitForEndOfFrame();
            PostProcessors.Remove(postProcess);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref IsCritical, "isCritical");
            Scribe_Values.Look(ref IsIncremental, "isIncremental");
            Scribe_Values.Look(ref Commitment, "commitment");
            Scribe_Deep.Look(ref MinWorkers, "minWorkers");
            Scribe_Deep.Look(ref TargetWorkers, "targetWorkers");
            Scribe_Deep.Look(ref Priorities, "priorities");
            Scribe_Collections.Look(ref Fitness, "fitness", LookMode.Deep);
            Scribe_Collections.Look(ref Conditions, "conditions", LookMode.Deep);
            Scribe_Collections.Look(ref PostProcessors, "postProcessors", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (MinWorkers == null) MinWorkers = new IntPawnAmount();
                if (TargetWorkers == null) TargetWorkers = new IntPawnAmount();
                if (Priorities == null) Priorities = PawnWorkPriorities.CreateDefault();
                if (Fitness == null) Fitness = new List<IPawnFitness>();
                if (Conditions == null) Conditions = new List<IPawnCondition>();
                if (PostProcessors == null) PostProcessors = new List<IPawnPostProcessor>();
            }
        }

        private class PawnFitnessComparer : IComparer<Pawn>
        {
            private List<IPawnFitness> _fitnessFunctions;
            private WorkSpecification _workSpec;
            private ResolveWorkRequest _workReq;

            public PawnFitnessComparer (List<IPawnFitness> funcs, WorkSpecification workSpec, ResolveWorkRequest workReq)
            {
                _fitnessFunctions = funcs;
                _workSpec = workSpec;
                _workReq = workReq;
            }

            public int Compare(Pawn x, Pawn y)
            {
                int funcIndex = 0;
                float diff = 0f;

                for (int i = 0; i <  _fitnessFunctions.Count; i++)
                {
                    float xFitnessd = _fitnessFunctions[funcIndex].CalcFitness(x, _workSpec, _workReq);
                    float yFitnessd = _fitnessFunctions[funcIndex].CalcFitness(y, _workSpec, _workReq);
                    diff = yFitnessd - xFitnessd;

                    if (Math.Abs(diff) > Mathf.Epsilon)
                        break;
                }

                return (int)Mathf.Sign(diff);
            }
        }
    }
}
