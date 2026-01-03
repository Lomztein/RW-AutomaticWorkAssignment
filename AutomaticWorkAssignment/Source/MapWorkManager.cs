using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.Source;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class MapWorkManager : MapComponent, IExposable
    {
        public Map Map { get; private set; }
        public Map ParentMap;

        public static MapWorkManager LastInitialized { get; private set; }
        public static Map LastInitializedMap => LastInitialized?.Map;

        private enum DefaultLoadType { Procedural, File, Gravship }

        public static MapWorkManager GetManager(Map map)
            => map.GetComponent<MapWorkManager>();
        public static MapWorkManager GetCurrentMapManager()
            => GetManager(Find.CurrentMap);

        public List<WorkSpecification> WorkList = new List<WorkSpecification>();

        private List<PawnRef> ExcludedPawns => MapPawnFilter.ExcludedPawns;

        private bool _refreshEachDayLegacy = false;

        public Dictionary<Pawn, List<WorkAssignment>> PawnAssignments = new Dictionary<Pawn, List<WorkAssignment>>();

        private int _lastResolveTick;

        public AutoResolveFrequencyDef ResolveFrequencyDef;
        public bool DoesAutoResolve => ResolveFrequencyDef != AutoResolveFrequencyUtils.None;

        private Cache<IEnumerable<Pawn>> _cachedPawns = new Cache<IEnumerable<Pawn>>();
        private Cache<IEnumerable<Map>> _allMaps = new Cache<IEnumerable<Map>>();

        private List<WorkTypeDef> _unmanagedWorkTypes;

        public static int MaxCommitment => AutomaticWorkAssignmentSettings.MaxCommitment;
        public static bool IgnoreUnmanagedWorkTypes => AutomaticWorkAssignmentSettings.IgnoreUnmanagedWorkTypes;

        public Reservations Reservations = new Reservations(); // Reserved items.
        public Dedications Dedications = new Dedications(); // Pawns dedicated to certain tasks.
        public MapPawnsFilter MapPawnFilter = new MapPawnsFilter();

        public MapWorkManager(Map map) : base(map)
        {
            Map = map;
            LastInitialized = this;
            LongEventHandler.QueueLongEvent(InitializeManager(), "AWA.InitializeManager");
        }

        private IEnumerable InitializeManager()
        {
            yield return new WaitForEndOfFrame();

            if (WorkList.Count == 0)
                ResetToDefaults();

            EnsureSanity();
        }

        private void EnsureSanity()
        {
            WorkList ??= new List<WorkSpecification>();
            Reservations ??= new Reservations();
            Dedications ??= new Dedications();
            ResolveFrequencyDef ??= AutoResolveFrequencyUtils.None;
            MapPawnFilter ??= new MapPawnsFilter();

            foreach (WorkSpecification spec in WorkList)
            {
                spec.Map = Map;
            }

            WorkList = WorkList.Where(x => x != null).ToList();
        }

        public void ResetToDefaults()
        {
            DefaultLoadType loadType = DetermineDefaultLoadType();
            if (loadType == DefaultLoadType.Procedural)
            {
                WorkList = Defaults.GenerateDefaultWorkSpecifications().ToList();
                MapPawnFilter = new MapPawnsFilter();
                ResolveFrequencyDef = AutoResolveFrequencyUtils.None;
                Log.Message("[AWA] Generated default work specs.");
            }
            if (loadType == DefaultLoadType.File)
            {
                FileInfo defaultConfig = AutomaticWorkAssignmentSettings.DefaultConfigurationFile;
                IO.ImportFromFile(this, defaultConfig.Name, IO.GetConfigDirectory());
                Log.Message($"[AWA] Imported from '{defaultConfig.Name}'.");
            }
            if (loadType == DefaultLoadType.Gravship)
            {
                string fileName = GravshipUtils.GravshipConfigMigrationFileName;
                IO.ImportFromFile(this, fileName, IO.GetGravshipConfigDirectory());
                Log.Message($"[AWA] Imported gravship file '{fileName}'.");
                GravshipUtils.GravshipConfigMigrationFileName = null;
            }

            EnsureSanity();
        }

        private DefaultLoadType DetermineDefaultLoadType()
        {
            if (AutomaticWorkAssignmentSettings.AutoMigrateOnGravshipJump && Map.wasSpawnedViaGravShipLanding && GravshipUtils.GravshipConfigMigrationFileExists())
                return DefaultLoadType.Gravship;

            FileInfo defaultConfig = AutomaticWorkAssignmentSettings.DefaultConfigurationFile;
            if (defaultConfig != null)
                return DefaultLoadType.File;

            return DefaultLoadType.Procedural;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (ParentMap == null)
            {
                bool shouldResolve = AutoResolveFrequencyUtils.ShouldResolveNow(ResolveFrequencyDef, _lastResolveTick, map);
                if (shouldResolve)
                {
                    _lastResolveTick = GenTicks.TicksAbs;
                    ResolveWorkAssignments();

                    Logger.Message("[AWA] Resolved work assignments.");
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
            if (_allMaps.TryGet(out IEnumerable<Map> maps))
                return maps;
            maps = CacheMaps();
            _allMaps.Set(maps);
            return maps;
        }

        private IEnumerable<Map> CacheMaps()
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
                foreach (var map in mapWorkManager.GetParentMaps())
                    yield return map;
            }
        }

        private IEnumerable<Pawn> CachePawns()
        {
            IEnumerable<Pawn> allPawns = GetAllMaps()
                .SelectMany(x => x.mapPawns.AllHumanlikeSpawned);
            return MapPawnFilter.GetEverAvailablePawns(allPawns, map);
        }

        public IEnumerable<Pawn> GetAllPawns()
        {
            return GetPawnCache().Where(x => x != null);
        }

        public IEnumerable<Pawn> GetAllAssignableNowPawns()
        {
            return GetPawnCache().Where(x => x != null && CanBeAssignedNow(x));
        }

        public IEnumerable<Pawn> GetAllEverAssignablePawns()
        {
            return GetPawnCache().Where(x => x != null && CanEverBeAssigned(x));
        }

        private IEnumerable<Pawn> GetPawnCache()
        {
            if (_cachedPawns.TryGet(out IEnumerable<Pawn> cachedPawns))
                return cachedPawns;
            cachedPawns = CachePawns();
            _cachedPawns.Set(cachedPawns);
            return cachedPawns;
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

        public bool IsTemporarilyUnavailable(Pawn pawn)
        {
            return !MapPawnFilter.RemoveTemporarilyUnavailablePawns(new Pawn[] { pawn }).Any();
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
            if (ExcludedPawns.Any(x => x.Is(pawn))) return false;
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

            foreach (WorkSpecification current in assignmentList)
            {
                // Assign dedicated pawns
                IEnumerable<Pawn> dedications = Dedications.GetDedicatedPawns(current);

                // Go over each work specification, find best fits, and assign work accordingly.
                var availablePawns = req.Pawns.Where(x => (current.IncludeSpecialists || !specialists.Contains(x)) && CanBeAssignedTo(x, current));
                IEnumerable<Pawn>matchesSorted = current.GetApplicableOrMinimalPawnsSorted(availablePawns, req);

                int currentAssigned = GetCountAssignedTo(current);
                int targetAssigned = current.GetTargetWorkers(req);
                int remaining = targetAssigned - currentAssigned;

                // Only assign the amount of available workers.
                int toAssign = remaining;

                float maxTargetCommitment = (1f - current.Commitment);
                List<Pawn> availableToAssign = matchesSorted.ToList();

                if (toAssign != 0)
                {
                    // Max commitment level increases if no pawns with enough available commitment was found.
                    for (int c = 0; c < maxCommitment; c++)
                    {
                        Queue<Pawn> commitable = new(dedications.Concat(availableToAssign.Where(x => GetPawnCommitment(x) < maxTargetCommitment + c)));

                        int i = 0;
                        int assigned = 0;
                        for (i = 0; i < toAssign; i++)
                        {
                            if (commitable.Count == 0)
                                break;

                            Pawn pawn = commitable.Dequeue();
                            AssignWorkToPawn(current, pawn);
                            availableToAssign.Remove(pawn);
                            assigned++;

                            // Add pawn to list of specialists, so that it may be excluded later.
                            if (current.IsSpecialist)
                                specialists.Add(pawn);
                        }
                        toAssign -= assigned;

                        if (toAssign == 0)
                        {
                            // Completed the for-loop, all assignents have been made, so we can move on.
                            break;
                        }
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
                if (spec.IsSuspended)
                    continue;

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
                    int shift = spec.InterweavePriorities ? GetPawnsAssignedTo(spec).FirstIndexOf(x => x == pawn) : 0;
                    var priorities = spec.Priorities.GetShifted(shift);

                    foreach (var priority in priorities)
                    {
                        int currentPriority = newPriorities[priority];

                        if (currentPriority == 0)
                        {
                            if (priority.naturalPriority > lastNatural)
                                prioritization++;
                            lastNatural = priority.naturalPriority;

                            if (!pawn.WorkTypeIsDisabled(priority))
                            {
                                newPriorities[priority] = prioritization;
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
            foreach (var countFrom in spec.CountAssigneesFrom)
            {
                if (AllowCountAssigneesFrom(spec, countFrom)) // Double check validity
                    num += GetCountAssignedTo(countFrom);
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
            foreach (var countAdditional in spec.CountAssigneesFrom)
            {
                if (AllowCountAssigneesFrom(spec, countAdditional))
                {
                    IEnumerable<Pawn> additional = GetPawnsAssignedTo(countAdditional);
                    foreach (Pawn pawn in additional)
                        yield return pawn;
                }
            }
        }

        public bool AllowCountAssigneesFrom(WorkSpecification spec, WorkSpecification from)
            => WorkList.IndexOf(spec) > WorkList.IndexOf(from);

        public void RemoveInvalidCountAssignessFrom(WorkSpecification spec)
        {
            List<WorkSpecification> invalid = new();
            foreach (var countFrom in spec.CountAssigneesFrom)
            {
                if (!AllowCountAssigneesFrom(spec, countFrom))
                    invalid.Add(countFrom);
            }
            foreach (var toRemove in invalid)
                spec.CountAssigneesFrom.Remove(toRemove);
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
            Scribe_Defs.Look(ref ResolveFrequencyDef, "resolveFrequencyDef");
            Scribe_Collections.Look(ref WorkList, "workSpecifications", LookMode.Deep);
            Scribe_Deep.Look(ref MapPawnFilter, "pawnFilter");
            Scribe_Deep.Look(ref Reservations, "reservations");
            Scribe_Deep.Look(ref Dedications, "dedications");
            Scribe_References.Look(ref ParentMap, "parentMap");

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                MapPawnFilter ??= new MapPawnsFilter();

                List<Pawn> legacyExludedPawns = null;
                Scribe_Collections.Look(ref legacyExludedPawns, "excludePawns", LookMode.Reference);
                if (legacyExludedPawns != null && legacyExludedPawns.Count > 0)
                    MapPawnFilter.ExcludedPawns = new List<PawnRef>(legacyExludedPawns.Where(x => x != null).Select(x => new PawnRef(x)));

                List<PawnRef> legacyExcludedPawnsAgain = null;
                Scribe_Collections.Look(ref legacyExcludedPawnsAgain, "excludedPawns", LookMode.Deep);
                if (legacyExcludedPawnsAgain != null && legacyExcludedPawnsAgain.Count > 0)
                    MapPawnFilter.ExcludedPawns = legacyExcludedPawnsAgain;

                Scribe_Values.Look(ref _refreshEachDayLegacy, "refreshEachDay", false);
                if (_refreshEachDayLegacy)
                    ResolveFrequencyDef = AutoResolveFrequencyUtils.Daily;
            }

            EnsureSanity();
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
            spec.Map = Map;
            WorkList.Add(spec);
            return spec;
        }

        public void AddWorkSpecification(WorkSpecification spec)
        {
            spec.Map = Map;
            WorkList.Add(spec);
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
            WorkList.Remove(spec);
        }
    }
}
