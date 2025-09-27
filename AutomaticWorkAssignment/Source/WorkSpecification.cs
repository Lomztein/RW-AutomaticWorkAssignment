using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkSpecification : IExposable, ILoadReferenceable
    {
        public string Name = "New Work"; // Label for UI.

        public bool IsCritical; // Job will temporarily be reassigned to another pawn if the prior assignee is unable to work.
        public bool RequireFullPawnCapability = true; // Job will not be assigned if a pawn is unable to do some of the work. If off, pawn must only be able to do at least one thing.
        public bool InterweavePriorities; // Subsequent assignments to pawns will be shifted right.
        public bool IsSpecialist; // Job will prohibit assignments to jobs further down the list.
        public bool IncludeSpecialists = false; // The job tries to be assigned according to the rules, even if the pawn was assigned to the job by a specialist
        public bool IsIgnoreCommitment = false; // Do not consider commitment when assigning pawns to the working specification. Use ONLY the fitness functions
        public bool IsSuspended;
        public bool EnableAlert = true;
        public float Commitment; // 0 = Occasional, 0.5 = part-time work, 1.0 = full-time work.

        public Map Map;

        public List<IPawnCondition> Conditions = new List<IPawnCondition>(); // Conditions required for a pawn to be valid. All conditions must be met.
        public List<IPawnFitness> Fitness = new List<IPawnFitness>(); // Used to sort pawns, later elements are used to break earlier ties.
        public List<IPawnPostProcessor> PostProcessors = new List<IPawnPostProcessor>(); // Used to do additional things to pawns assigned to this work.

        public IPawnAmount MinWorkers = PawnAmount.CreateFrom(PawnAmountDefOf.Lomzie_IntPawnAmount); // Min workers, conditions will be ignored if applicable are below this.
        public IPawnAmount TargetWorkers = PawnAmount.CreateFrom(PawnAmountDefOf.Lomzie_IntPawnAmount); // Number of workers to be assigned this particular work.

        public PawnWorkPriorities Priorities = PawnWorkPriorities.CreateEmpty(); // The actual work priorities to be assigned.

        public List<WorkSpecification> CountAssigneesFrom = new List<WorkSpecification>();

        public Pawn[] GetApplicableOrMinimalPawnsSorted(IEnumerable<Pawn> allPawns, ResolveWorkRequest request)
        {
            PawnFitnessComparer comparer = new PawnFitnessComparer(Fitness, this, request);

            // Sort by fitness.
            var arr = GetApplicableOrMinimalPawns(allPawns, request).ToArray();
            Array.Sort(arr, comparer);

            return arr;
        }

        public IEnumerable<Pawn> GetApplicableOrMinimalPawns(IEnumerable<Pawn> allPawns, ResolveWorkRequest request)
        {
            // Find applicable pawns.
            var applicable = GetApplicablePawns(allPawns, request);
            int applicableCount = applicable.Count();
            int minCount = MinWorkers.GetCount(this, request);
            if (applicableCount < minCount)
            {
                int missing = minCount - applicableCount;
                var substitutesSorted = allPawns.Where(x => !applicable.Contains(x) && CanPawnDoWork(x)).ToArray();

                PawnFitnessComparer comparer = new PawnFitnessComparer(Fitness, this, request);
                Array.Sort(substitutesSorted, comparer);

                var toSubstitute = substitutesSorted.ToList().GetRange(0, Mathf.Min(missing, substitutesSorted.Length));

                return Enumerable.Concat(applicable, toSubstitute).ToArray();
            }

            return applicable;
        }

        public IEnumerable<Pawn> GetApplicablePawns(IEnumerable<Pawn> allPawns, ResolveWorkRequest request)
        {
            foreach (Pawn pawn in allPawns)
            {
                if (Conditions.All(x => x.IsValid(pawn, this, request)))
                {
                    if (CanPawnDoWork(pawn))
                        yield return pawn;
                }
            }
        }

        public bool CanPawnDoWork(Pawn pawn)
        {
            if (RequireFullPawnCapability)
            {
                if (!pawn.OneOfWorkTypesIsDisabled(Priorities.OrderedPriorities))
                    return true;
            }
            else
            {
                if (Priorities.OrderedPriorities.Any(x => !pawn.WorkTypeIsDisabled(x)) || Priorities.OrderedPriorities.Empty())
                    return true;
            }
            return false;
        }

        public int GetTargetWorkers(ResolveWorkRequest request)
            => Math.Max(TargetWorkers.GetCount(this, request), MinWorkers.GetCount(this, request));

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

        public void ReplaceFitness(IPawnFitness fitness, IPawnFitness newFitness)
            => Find.Root.StartCoroutine(DelayedReplaceFitness(fitness, newFitness));

        private IEnumerator DelayedReplaceFitness(IPawnFitness fitness, IPawnFitness newFitness)
        {
            yield return new WaitForEndOfFrame();
            Utils.ReplaceElement(Fitness, fitness, newFitness);
        }

        public void MoveCondition(IPawnCondition condition, int movement)
            => Find.Root.StartCoroutine(DelayedMoveCondition(condition, movement));

        private IEnumerator DelayedMoveCondition(IPawnCondition condition, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(Conditions, condition, movement);
        }

        public void DeleteCondition(IPawnCondition condition)
            => Find.Root.StartCoroutine(DelayedDeleteCondition(condition));

        private IEnumerator DelayedDeleteCondition(IPawnCondition condition)
        {
            yield return new WaitForEndOfFrame();
            Conditions.Remove(condition);
        }

        public void ReplaceCondition(IPawnCondition condition, IPawnCondition newCondition)
            => Find.Root.StartCoroutine(DelayedReplaceCondition(condition, newCondition));

        private IEnumerator DelayedReplaceCondition(IPawnCondition condition, IPawnCondition newCondition)
        {
            yield return new WaitForEndOfFrame();
            Utils.ReplaceElement(Conditions, condition, newCondition);
        }

        public void MovePostProcessor(IPawnPostProcessor postProcess, int movement)
         => Find.Root.StartCoroutine(DelayedMovePostProcess(postProcess, movement));

        private IEnumerator DelayedMovePostProcess(IPawnPostProcessor postProcess, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(PostProcessors, postProcess, movement);
        }

        public void DeletePostProcessor(IPawnPostProcessor postProcess)
            => Find.Root.StartCoroutine(DelayedDeletePostProcess(postProcess));

        private IEnumerator DelayedDeletePostProcess(IPawnPostProcessor postProcess)
        {
            yield return new WaitForEndOfFrame();
            PostProcessors.Remove(postProcess);
        }

        public void ReplacePostProcess(IPawnPostProcessor postProcess, IPawnPostProcessor newPostProcessor)
            => Find.Root.StartCoroutine(DelayedReplacePostProcessor(postProcess, newPostProcessor));

        private IEnumerator DelayedReplacePostProcessor(IPawnPostProcessor postProcess, IPawnPostProcessor newPostProcessor)
        {
            yield return new WaitForEndOfFrame();
            Utils.ReplaceElement(PostProcessors, postProcess, newPostProcessor);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref IsCritical, "isCritical");
            Scribe_Values.Look(ref IsSpecialist, "isSpecialist");
            Scribe_Values.Look(ref IncludeSpecialists, "isIgnoreSpecialist");
            Scribe_Values.Look(ref IsIgnoreCommitment, "isIgnoreCommitment");
            Scribe_Values.Look(ref IsSuspended, "isSuspended");
            Scribe_Values.Look(ref EnableAlert, "enableAlert", true);
            Scribe_Values.Look(ref RequireFullPawnCapability, "requireFullPawnCapability");
            Scribe_Values.Look(ref InterweavePriorities, "interweavePriorities");
            Scribe_Values.Look(ref Commitment, "commitment");
            Scribe_Deep.Look(ref MinWorkers, "minWorkers");
            Scribe_Deep.Look(ref TargetWorkers, "targetWorkers");
            Scribe_Deep.Look(ref Priorities, "priorities");
            Scribe_Collections.Look(ref Fitness, "fitness", LookMode.Deep);
            Scribe_Collections.Look(ref Conditions, "conditions", LookMode.Deep);
            Scribe_Collections.Look(ref PostProcessors, "postProcessors", LookMode.Deep);
            Scribe_Collections.Look(ref CountAssigneesFrom, "countAssigneesFrom", LookMode.Reference, true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (MinWorkers == null) MinWorkers = PawnAmount.CreateFrom(PawnAmountDefOf.Lomzie_IntPawnAmount);
                if (TargetWorkers == null) TargetWorkers = PawnAmount.CreateFrom(PawnAmountDefOf.Lomzie_IntPawnAmount);
                if (Priorities == null) Priorities = PawnWorkPriorities.CreateDefault();
                if (Fitness == null) Fitness = new List<IPawnFitness>();
                if (Conditions == null) Conditions = new List<IPawnCondition>();
                if (PostProcessors == null) PostProcessors = new List<IPawnPostProcessor>();
                if (CountAssigneesFrom == null) CountAssigneesFrom = new List<WorkSpecification>();

                Fitness = Fitness.ToList().Where(x => x.IsValidAfterLoad()).ToList();
                Conditions = Conditions.ToList().Where(x => x.IsValidAfterLoad()).ToList();
                PostProcessors = PostProcessors.ToList().Where(x => x.IsValidAfterLoad()).ToList();
                CountAssigneesFrom = CountAssigneesFrom.Where(x => x != null).ToList();
            }
        }

        public string GetUniqueLoadID()
        {
            int uniqueId = Name.GetHashCode() *
                (Priorities.OrderedPriorities.Count + 6516) *
                (Fitness.Count + 4754) *
                (Conditions.Count + 7988) *
                (PostProcessors.Count + 6874);

            if (Map != null)
                uniqueId += Map.uniqueID;

            return $"WorkSpec_{uniqueId}";
        }
    }
}
