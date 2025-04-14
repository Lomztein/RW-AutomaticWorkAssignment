using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Drawing;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkManager : GameComponent
    {
        public static WorkManager Instance;

        public List<WorkSpecification> WorkList = new List<WorkSpecification>();
        public List<Pawn> ExcludePawns = new List<Pawn>();

        public bool RefreshEachDay = true;

        public Dictionary<Pawn, List<WorkAssignment>> PawnAssignments = new Dictionary<Pawn, List<WorkAssignment>>();

        private int _lastResolveDay;

        private int _lastCachePawnsTick;
        private readonly int _cachePawnsThreshold = 300;
        private IEnumerable<Pawn> _cachedPawns;

        private bool IsPawnCacheDirty => GenTicks.TicksGame > _lastCachePawnsTick + _cachePawnsThreshold || _cachedPawns == null;

        public WorkManager(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (RefreshEachDay)
            {
                int currentDay = GenDate.DaysPassed;
                if (currentDay > _lastResolveDay)
                {
                    _lastResolveDay = currentDay;
                    ResolveWorkAssignments();
                }
            }
        }

        public void ResolveWorkAssignments ()
        {
            ResolveWorkCoroutine(MakeDefaultRequest());
        }

        public ResolveWorkRequest MakeDefaultRequest ()
        {
            var pawns = GetAllAssignablePawns().ToList();
            return new ResolveWorkRequest() { Pawns = pawns, Map = GetCurrentMap(), WorkManager = this };
        }

        public IEnumerable<Map> GetAllMaps ()
        {
            return Find.Maps;
        }

        private void CachePawns ()
        {
            _cachedPawns = GetAllMaps()
                .SelectMany(x => x.mapPawns.FreeColonistsAndPrisoners)
                .Where(x => x != null && (x.IsFreeNonSlaveColonist || x.IsSlaveOfColony));
            _lastCachePawnsTick = GenTicks.TicksGame;
        }

        public IEnumerable<Pawn> GetAllPawns()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawns.Where(x => x != null);
        }

        public IEnumerable<Pawn> GetAllAssignablePawns()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawns.Where(x => x != null && CanBeAssigned(x));
        }

        public int GetPawnCount()
            => GetAllPawns().Count();

        public int GetAssignablePawnCount()
            => GetAllAssignablePawns().Count();

        public void ResolveWorkCoroutine(ResolveWorkRequest req)
        {
            ResolveAssignments(req);
            ResolvePriorities(req);
            PostProcessAssignments(req);
        }

        public WorkAssignment GetAssignmentTo(Pawn pawn, WorkSpecification spec)
        {
            if (PawnAssignments.TryGetValue(pawn, out var assignments))
            {
                return assignments.FirstOrDefault(x => x.Specification == spec);
            }
            return null;
        }

        private void PostProcessAssignments(ResolveWorkRequest req)
        {
            foreach (var assignment in PawnAssignments)
            {
                Pawn pawn = assignment.Key;
                List<WorkAssignment> assignments = assignment.Value;

                foreach (var workAssignment in assignments)
                {
                    workAssignment.Specification.ApplyPostProcessing(pawn, req);
                }
            }
        }

        public bool CanBeAssignedTo(Pawn pawn, WorkSpecification workSpecification)
        {
            if (!CanBeAssigned(pawn)) return false;
            if (IsAssignedTo(pawn, workSpecification)) return false;
            return true;
        }

        public bool CanBeAssigned(Pawn pawn)
        {
            if (pawn == null) return false;
            if (ExcludePawns.Contains(pawn)) return false;
            if (pawn.DeadOrDowned) return false;
            return true;
        }

        private Map GetCurrentMap()
            => Find.CurrentMap;

        private void ResolveAssignments (ResolveWorkRequest req)
        {
            int specIndex = 0;
            int maxCommitment = 25;

            ClearAllAssignments();
            List<WorkSpecification> assignmentList = new List<WorkSpecification>(WorkList);

            while (assignmentList.Count > 0)
            {
                // Go over each work specification, find best fits, and assign work accordingly.
                WorkSpecification current = assignmentList[specIndex];
                IEnumerable<Pawn> matchesSorted = current.GetApplicablePawnsSorted(req.Pawns, req);
                matchesSorted = matchesSorted.Where(x => CanBeAssignedTo(x, current));

                int currentAssigned = GetCountAssignedTo(current);
                int targetAssigned = current.GetTargetWorkers();
                int remaining = targetAssigned - currentAssigned;

                int toAssign = current.IsIncremental ? Mathf.Min(1, remaining) : remaining;
                float maxTargetCommitment = (1f - current.Commitment);

                for (int c = 0; c < maxCommitment; c++) // Max commitment level increases if no pawns with enough available commitment was found.
                {
                    Queue<Pawn> commitable = new Queue<Pawn>(matchesSorted.Where(x => GetPawnCommitment(x) < maxTargetCommitment + c).ToList());

                    int i = 0;
                    for (i = 0; i < toAssign; i++)
                    {
                        if (commitable.Count == 0)
                            break;

                        AssignWorkToPawn(current, commitable.Dequeue());
                        toAssign--;
                    }

                    if (c == maxCommitment - 1)
                    {
                        assignmentList.Remove(current); // Not able to find a suitable worker.
                    }

                    if (i >= toAssign)
                    {
                        // Completed the for-loop, all assignents have been made, so we can move on.
                        break;
                    }
                }

                int postAssignmentCount = GetCountAssignedTo(current);
                if (targetAssigned <= postAssignmentCount)
                    assignmentList.Remove(current); // Job is satisfied.
            }
        }

        private void ResolvePriorities(ResolveWorkRequest req)
        {
            foreach (var pawn in req.Pawns)
            {
                ResolvePawnPriorities(pawn);
            }
        }

        public void ResolvePawnPriorities(Pawn pawn)
        {
            foreach (var def in DefDatabase<WorkTypeDef>.AllDefs)
            {
                pawn.workSettings?.SetPriority(def, 0);
            }

            if (PawnAssignments.TryGetValue(pawn, out List<WorkAssignment> assignments))
            {
                var specs = assignments.Select(x => x.Specification);

                int lastNatural = int.MaxValue;
                int prioritization = 1;

                foreach (var spec in specs)
                {
                    for (int i = 0; i < spec.Priorities.OrderedPriorities.Count; i++)
                    {
                        WorkTypeDef curDef = spec.Priorities.OrderedPriorities[i];
                        int currentPriority = pawn.workSettings.GetPriority(curDef);

                        if (currentPriority == 0)
                        {
                            if (curDef.naturalPriority > lastNatural)
                                prioritization++;
                            lastNatural = curDef.naturalPriority;

                            if (!pawn.WorkTypeIsDisabled(curDef))
                            {
                                pawn.workSettings?.SetPriority(curDef, prioritization);
                            }
                        }
                    }
                }
            }
        }

        public int GetCountAssignedTo(WorkSpecification spec)
        {
            int num = 0;
            foreach (var kvp in PawnAssignments)
            {
                num += kvp.Value.Count(x => x.Specification == spec);
            }
            return num;
        }

        public IEnumerable<Pawn> GetPawnsAssignedTo(WorkSpecification spec)
        {
            foreach (var kvp in PawnAssignments)
            {
                if (kvp.Value.Any(x => x.Specification == spec))
                    yield return kvp.Key;
            }
        }


        public bool IsAssignedTo(Pawn pawn, WorkSpecification spec)
        {
            if (PawnAssignments.TryGetValue(pawn, out var assignedTo))
            {
                return assignedTo.Any(x => x.Specification == spec);
            }
            return false;
        }

        public float GetPawnCommitment(Pawn pawn)
        {
            if (PawnAssignments.ContainsKey(pawn))
                return PawnAssignments[pawn].Sum(x => x.Specification.Commitment);
            return 0f;
        }

        public bool IsWorkSpecificationMinimallySatisfied(WorkSpecification spec)
        {
            int numAssigned = GetCountAssignedTo(spec);
            int target = spec.MinWorkers.GetCount();
            return numAssigned >= target;
        }

        public bool IsWorkSpecificationSatisfied(WorkSpecification spec)
        {
            int numAssigned = GetCountAssignedTo(spec);
            int target = spec.GetTargetWorkers();
            if (numAssigned == target) return true;
            if (numAssigned > target)
            {
                Log.WarningOnce($"Work specification {spec.Name} assigned to more than requested.", "WorkSpecOverAssigned".GetHashCode());
                return true;
            }
            return false;
        }

        public void TransferAssignment(Pawn from, Pawn to, WorkAssignment assignment)
        {
        }

        public void ClearAllAssignments()
        {
            PawnAssignments.Clear();
        }

        public void ClearPawnAssignments(Pawn pawn)
        {
            if (PawnAssignments.ContainsKey(pawn))
                PawnAssignments[pawn].Clear();
        }

        public WorkAssignment AssignWorkToPawn(WorkSpecification spec, Pawn pawn, int index = -1)
        {
            if (!PawnAssignments.ContainsKey(pawn))
                PawnAssignments.Add(pawn, new List<WorkAssignment>());
            if (index == -1) index = PawnAssignments[pawn].Count;

            WorkAssignment assignment = new WorkAssignment(spec, pawn, index, spec.IsCritical);
            PawnAssignments[pawn].Insert(index, assignment);
            return assignment;
        }

        public override void ExposeData ()
        {
            Scribe_Collections.Look(ref WorkList, "workSpecifications", LookMode.Deep);
            Scribe_Collections.Look(ref ExcludePawns, "excludePawns", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (WorkList == null) WorkList = new List<WorkSpecification>();
                if (ExcludePawns == null) ExcludePawns = new List<Pawn>();
            }
        }

        public void RemoveAssignmentFromPawn(WorkAssignment assignment, Pawn pawn)
        {
        }

        public WorkSpecification CreateNewWorkSpecification()
        {
            WorkSpecification spec = new WorkSpecification();
            WorkList.Add(spec);
            return spec;
        }

        public void RemoveWorkSpecification(WorkSpecification spec)
            => WorkList.Remove(spec);

        public void MoveWorkSpecification(WorkSpecification spec, int movement)
        {
            Find.Root.StartCoroutine(MoveWorkSpecAtEndOfFrame(spec, movement));
        }

        private IEnumerator MoveWorkSpecAtEndOfFrame(WorkSpecification spec, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(WorkList, spec, movement);
        }

        public void DeleteWorkSpecification(WorkSpecification spec)
        {
            Find.Root.StartCoroutine(DelayedDeleteWorkSpecification(spec));
        }

        private IEnumerator DelayedDeleteWorkSpecification(WorkSpecification spec)
        {
            yield return new WaitForEndOfFrame();
            WorkList.Remove(spec); ;
        }
    }
}