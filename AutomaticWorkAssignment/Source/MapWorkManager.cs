using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class MapWorkManager : MapComponent, IExposable
    {
        public Map Map { get; private set; }
        public Map ParentMap;

        public static MapWorkManager GetManager(Map map)
            => map.GetComponent<MapWorkManager>();
        public static MapWorkManager GetCurrentMapManager()
            => GetManager(Find.CurrentMap);

        public List<WorkSpecification> WorkList = new List<WorkSpecification>();
        public List<Pawn> ExcludePawns = new List<Pawn>();

        public bool RefreshEachDay = false;

        public Dictionary<Pawn, List<WorkAssignment>> PawnAssignments = new Dictionary<Pawn, List<WorkAssignment>>();

        private int _lastResolveDay;

        private int _lastCachePawnsTick;
        private readonly int _cachePawnsThreshold = GenTicks.TickLongInterval;
        private IEnumerable<Pawn> _cachedPawns;
        private List<WorkTypeDef> _unmanagedWorkTypes;
        private bool IsPawnCacheDirty => GenTicks.TicksGame > _lastCachePawnsTick + _cachePawnsThreshold || _cachedPawns == null;

        public static int MaxCommitment => AutomaticWorkAssignmentSettings.MaxCommitment;
        public static float MaxMentalBreakHours => AutomaticWorkAssignmentSettings.MentalBreakHourThreshold;
        public static bool IgnoreUnmanagedWorkTypes => AutomaticWorkAssignmentSettings.IgnoreUnmanagedWorkTypes;

        public Reservations Reservations = new Reservations();

        public MapWorkManager(Map map) : base(map)
        {
            Map = map;
            LongEventHandler.QueueLongEvent(InitializeManager(), "AWA.InitializeManager");
            InitializeManager();
        }

        private IEnumerable InitializeManager()
        {
            yield return new WaitForEndOfFrame();
            WorkManager legacyManager = WorkManager.GetLegacyManager();
            if (legacyManager != null && legacyManager.WorkList.Count > 0)
            {
                WorkList = new List<WorkSpecification>(legacyManager.WorkList);
                ExcludePawns = new List<Pawn>(legacyManager.ExcludePawns);
                RefreshEachDay = legacyManager.RefreshEachDay;
                Log.Message("[AWA] Migrated legacy work specs to map components.");
            }
            else if (WorkList.Count == 0)
            {
                WorkList = Defaults.GenerateDefaultWorkSpecifications().ToList();
                Log.Message("[AWA] Generated default work specs.");
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (RefreshEachDay && ParentMap == null)
            {
                int currentDay = GenDate.DaysPassed;
                if (currentDay > _lastResolveDay)
                {
                    _lastResolveDay = currentDay;
                    ResolveWorkAssignments();
                }
            }
        }

        public void ResolveWorkAssignments()
        {
            ResolveWorkCoroutine(MakeDefaultRequest());
        }

        public ResolveWorkRequest MakeDefaultRequest()
        {
            var pawns = GetAllAssignableNowPawns().ToList();
            return new ResolveWorkRequest() { Pawns = pawns, Map = GetCurrentMap(), WorkManager = this };
        }

        public IEnumerable<Map> GetChildMaps()
            => Find.Maps.Where(x => x.GetComponent<MapWorkManager>().ParentMap == Map);


        public IEnumerable<Map> GetAllMaps()
        {
            Map rootMap = Map;

            yield return rootMap;
            foreach (var child in GetChildMaps())
            {
                MapWorkManager manager = child.GetComponent<MapWorkManager>();
                foreach (var childMap in manager.GetAllMaps())
                {
                    if (childMap == rootMap)
                    {
                        ParentMap = null;
                        Messages.Message("Work manager parent loop detected. This is not supposed to happen, please report this on the workshop page. Parent link has been cut to avoid recursive loops.", MessageTypeDefOf.NegativeEvent);
                        throw new InvalidOperationException("Map parenting loop detected!");
                    }
                    yield return childMap;
                }
            }
        }

        public IEnumerable<Map> GetParentMaps()
        {
            if (ParentMap != null)
            {
                yield return ParentMap;
                MapWorkManager mapWorkManager = ParentMap.GetComponent<MapWorkManager>();
                foreach (var map in  mapWorkManager.GetParentMaps())
                    yield return map;
            }
        } 

        private void CachePawns()
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

        public IEnumerable<Pawn> GetAllAssignableNowPawns()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawns.Where(x => x != null && CanBeAssignedNow(x));
        }

        public IEnumerable<Pawn> GetAllEverAssignablePawns()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawns.Where(x => x != null && CanEverBeAssigned(x));
        }

        public int GetPawnCount()
            => GetAllPawns().Count();

        public int GetAssignablePawnCount()
            => GetAllAssignableNowPawns().Count();

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

        public static bool IsTemporarilyUnavailable(Pawn pawn)
        {
            return pawn != null && (IsMentalStateBlocking(pawn) || pawn.Downed || pawn.InCryptosleep);
        }

        private static bool IsMentalStateBlocking(Pawn pawn)
        {
            if (pawn != null && pawn.MentalStateDef != null)
            {
                if (pawn.MentalStateDef.maxTicksBeforeRecovery > GenDate.TicksPerHour * MaxMentalBreakHours) return true;
                if (pawn.MentalStateDef.IsExtreme) return true;
                if (pawn.MentalStateDef.IsAggro) return true;
            }
            return false;
        }

        public bool CanBeAssignedTo(Pawn pawn, WorkSpecification workSpecification)
        {
            if (!CanBeAssignedNow(pawn)) return false;
            if (IsAssignedTo(pawn, workSpecification)) return false;
            return true;
        }

        public bool CanBeAssignedNow(Pawn pawn)
        {
            if (!CanEverBeAssigned(pawn)) return false;
            if (IsTemporarilyUnavailable(pawn)) return false;
            return true;
        }

        public bool CanEverBeAssigned(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.Dead) return false;
            if (ExcludePawns.Contains(pawn)) return false;
            return true;
        }

        private Map GetCurrentMap()
            => Find.CurrentMap;

        private void ResolveAssignments(ResolveWorkRequest req)
        {
            int maxCommitment = Mathf.Clamp(1, MaxCommitment, 25);

            ClearAllAssignments();
            List<WorkSpecification> assignmentList = WorkList.Where(x => !x.IsSuspended).ToList();
            List<Pawn> specialists = new List<Pawn>();

            while (assignmentList.Count > 0)
            {
                // Go over each work specification, find best fits, and assign work accordingly.
                WorkSpecification current = assignmentList[0];
                IEnumerable<Pawn> matchesSorted = current.GetApplicableOrMinimalPawnsSorted(req.Pawns, req);
                matchesSorted = matchesSorted.Where(x => CanBeAssignedTo(x, current) && !specialists.Contains(x));

                int currentAssigned = GetCountAssignedTo(current);
                int targetAssigned = current.GetTargetWorkers(req);
                int remaining = targetAssigned - currentAssigned;

                // If incremenetal, assign one pawn per while iteration.
                int toAssign = current.IsIncremental ? Mathf.Min(1, remaining) : remaining;
                // Only assign the amount of available workers.
                int canAssign = matchesSorted.Count();
                toAssign = Mathf.Min(toAssign, canAssign);

                float maxTargetCommitment = (1f - current.Commitment);

                if (canAssign != 0)
                {
                    // Max commitment level increases if no pawns with enough available commitment was found.
                    for (int c = 0; c < maxCommitment; c++)
                    {
                        Queue<Pawn> commitable = new Queue<Pawn>(matchesSorted.Where(x => GetPawnCommitment(x) < maxTargetCommitment + c).ToList());

                        int i = 0;
                        int assigned = 0;
                        for (i = 0; i < toAssign; i++)
                        {
                            if (commitable.Count == 0)
                                break;

                            Pawn pawn = commitable.Dequeue();
                            AssignWorkToPawn(current, pawn);
                            assigned++;

                            // Add pawn to list of specialists, so that it may be excluded later.
                            if (current.IsSpecialist)
                                specialists.Add(pawn);
                        }
                        toAssign -= assigned;

                        if (c >= maxCommitment - 1)
                        {
                            // Not able to find a suitable commitable worker.
                            assignmentList.Remove(current);
                        }

                        if (toAssign == 0)
                        {
                            // Completed the for-loop, all assignents have been made, so we can move on.
                            break;
                        }
                    }
                }
                else
                {
                    // There are no more applicable workers for this work, remove it from the list.
                    assignmentList.Remove(current);
                }

                int postAssignmentCount = GetCountAssignedTo(current);
                if (targetAssigned <= postAssignmentCount)
                {
                    assignmentList.Remove(current); // Job is satisfied.
                    if (targetAssigned < postAssignmentCount)
                        Log.Warning($"{current.Name} has been over-assigned!");
                }
                // Work spec is not fully satisfied, move to end of list and try again next iteration.
                else if (assignmentList.Count > 0)
                {
                    // Only move the actual current spec to the back of the list, in case we accidentally removed it earlier.
                    if (assignmentList.Remove(current))
                    {
                        assignmentList.Add(current);
                    }
                }
            }
        }

        private void ResolvePriorities(ResolveWorkRequest req)
        {
            _unmanagedWorkTypes = GetUnmanagedWorkTypes();
            foreach (var pawn in req.Pawns)
            {
                ResolvePawnPriorities(pawn);
            }
        }

        private List<WorkTypeDef> GetUnmanagedWorkTypes()
        {
            List<WorkTypeDef> all = new List<WorkTypeDef>(DefDatabase<WorkTypeDef>.AllDefs);
            foreach (var spec in WorkList)
            {
                List<WorkTypeDef> toRemove = new List<WorkTypeDef>();
                foreach (WorkTypeDef def in all)
                {
                    if (spec.Priorities.OrderedPriorities.Contains(def))
                        toRemove.Add(def);
                }
                foreach (WorkTypeDef def in toRemove)
                {
                    all.Remove(def);
                }
            }
            return all;
        }

        private bool ShouldIgnoreWorkType(WorkTypeDef workTypeDef)
        {
            if (IgnoreUnmanagedWorkTypes)
            {
                return _unmanagedWorkTypes.Contains(workTypeDef);
            }
            return false;
        }

        public void ResolvePawnPriorities(Pawn pawn)
        {
            var workList = DefDatabase<WorkTypeDef>.AllDefs.ToList();
            workList.SortBy(x => x.naturalPriority); // Shouldn't actually matter

            Dictionary<WorkTypeDef, int> newPriorities = new Dictionary<WorkTypeDef, int>();
            foreach (var def in workList)
            {
                if (!ShouldIgnoreWorkType(def))
                {
                    newPriorities.Add(def, 0);
                }
                else
                {
                    newPriorities.Add(def, pawn.workSettings.GetPriority(def));
                }
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
                        int currentPriority = newPriorities[curDef];

                        if (currentPriority == 0)
                        {
                            if (curDef.naturalPriority > lastNatural)
                                prioritization++;
                            lastNatural = curDef.naturalPriority;

                            if (!pawn.WorkTypeIsDisabled(curDef))
                            {
                                newPriorities[curDef] = prioritization;
                            }
                        }
                    }
                }
            }

            foreach (var kvp in newPriorities)
            {
                if (pawn.workSettings.GetPriority(kvp.Key) != kvp.Value)
                    pawn.workSettings.SetPriority(kvp.Key, kvp.Value);
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

        public bool CanWorkSpecificationBeMinimallySatisfiedWithApplicablePawns(WorkSpecification spec, ResolveWorkRequest request)
        {
            ResolveWorkRequest req = MakeDefaultRequest();
            int numApplicable = spec.GetApplicablePawns(req.Pawns, req).Count();
            int target = spec.MinWorkers.GetCount(spec, request);
            return numApplicable >= target;
        }

        public bool IsWorkSpecificationSatisfied(WorkSpecification spec, ResolveWorkRequest request)
        {
            int numAssigned = GetCountAssignedTo(spec);
            int target = spec.GetTargetWorkers(request);
            if (numAssigned == target) return true;
            if (numAssigned > target)
            {
                Log.WarningOnce($"Work specification {spec.Name} assigned to more than requested.", "WorkSpecOverAssigned".GetHashCode());
                return true;
            }
            return false;
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
            index = Mathf.Clamp(index, 0, PawnAssignments[pawn].Count);

            WorkAssignment assignment = new WorkAssignment(spec, pawn, index, spec.IsCritical);
            PawnAssignments[pawn].Insert(index, assignment);
            return assignment;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref RefreshEachDay, "refreshEachDay", false);
            Scribe_Collections.Look(ref WorkList, "workSpecifications", LookMode.Deep);
            Scribe_Collections.Look(ref ExcludePawns, "excludePawns", LookMode.Reference);
            Scribe_Deep.Look(ref Reservations, "reservations");
            Scribe_References.Look(ref ParentMap, "parentMap");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (WorkList == null) WorkList = new List<WorkSpecification>();
                if (ExcludePawns == null) ExcludePawns = new List<Pawn>();
                if (Reservations == null) Reservations = new Reservations();
            }
        }

        public void RemoveAssignmentFromPawn(WorkAssignment assignment, Pawn pawn)
        {
            if (PawnAssignments.TryGetValue(pawn, out var list))
            {
                list.Remove(assignment);
            }
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