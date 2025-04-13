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
        private int _cachedPawnCount;

        private bool IsPawnCacheDirty => GenTicks.TicksGame > _lastCachePawnsTick + _cachePawnsThreshold;

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
            var pawns = GetAllPawns().ToList();
            ResolveWorkCoroutine(new ResolveWorkRequest() { Pawns = pawns, Map = GetCurrentMap(), WorkManager = this });
        }

        public IEnumerable<Map> GetAllMaps ()
        {
            return Find.Maps;
        }

        private void CachePawns ()
        {
            _cachedPawns = GetAllMaps().SelectMany(x => x.mapPawns.FreeColonists);
            _cachedPawnCount = _cachedPawns.Count();
            _lastCachePawnsTick = GenTicks.TicksGame;
        }

        public IEnumerable<Pawn> GetAllPawns()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawns.Where(x => x != null);
        }

        public int GetPawnCount()
        {
            if (IsPawnCacheDirty)
                CachePawns();
            return _cachedPawnCount;
        }

        public void ResolveWorkCoroutine(ResolveWorkRequest req)
        {
            ResolveAssignments(req);
            ResolvePriorities(req);
            PostProcessAssignments(req);
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

        private bool CanBeAssignedTo(Pawn pawn, WorkSpecification workSpecification)
        {
            if (pawn == null) return false;
            if (IsAssignedTo(pawn, workSpecification)) return false;
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


                int toAssign = remaining;
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

        private void ResolvePawnPriorities(Pawn pawn)
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

        public void AssignWorkToPawn(WorkSpecification spec, Pawn pawn, int index = -1)
        {
            if (!PawnAssignments.ContainsKey(pawn))
                PawnAssignments.Add(pawn, new List<WorkAssignment>());
            PawnAssignments[pawn].Add(new WorkAssignment(spec, pawn));
        }

        public override void ExposeData ()
        {
            Scribe_Collections.Look(ref WorkList, "workSpecifications", LookMode.Deep);
            Scribe_Collections.Look(ref ExcludePawns, "excludePawns", LookMode.Value);

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

        private List<WorkSpecification> GenerateTestWorkSpecifications ()
        {
            // Researcher
            WorkSpecification researcher = new WorkSpecification();
            researcher.Name = "Researcher";
            researcher.TargetWorkers = new IntPawnAmount() { Value = 3 };
            researcher.Commitment = 1f;
            researcher.Fitness.Add(new LearnRatePawnFitness());
            researcher.Fitness.Add(new SkillLevelPawnFitness());
            researcher.PostProcessors.Add(new SetTitlePawnPostProcessor());
            researcher.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Research);
            researcher.Priorities.OrderedPriorities.Add(WorkTypeDefOf.DarkStudy);
            Log.Message("Researcher!");

            // Doctor
            WorkSpecification doctorAssignment = new WorkSpecification();
            doctorAssignment.Name = "Doctor";
            doctorAssignment.TargetWorkers = new PercentagePawnAmount() { Percentage = 1f };
            doctorAssignment.IsCritical = true;
            doctorAssignment.MinWorkers = new IntPawnAmount() { Value = 2 };
            doctorAssignment.Commitment = 0f;
            doctorAssignment.Conditions.Add(new SkillLevelPawnCondition() { MinLevel = 10, SkillDef = SkillDefOf.Medicine });
            doctorAssignment.Fitness.Add(new LearnRatePawnFitness());
            doctorAssignment.PostProcessors.Add(new SetTitlePawnPostProcessor());
            doctorAssignment.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Doctor);
            Log.Message("Doctor!");

            // Socialite
            WorkSpecification socialAssignment = new WorkSpecification();
            socialAssignment.Name = "Socialite";
            socialAssignment.TargetWorkers = new IntPawnAmount() { Value = 2 };
            socialAssignment.IsCritical = true;
            socialAssignment.Commitment = 0.5f;
            socialAssignment.Fitness.Add(new LearnRatePawnFitness());
            socialAssignment.Fitness.Add(new SkillLevelPawnFitness());
            socialAssignment.PostProcessors.Add(new SetTitlePawnPostProcessor());
            socialAssignment.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Childcare);
            socialAssignment.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Warden);
            socialAssignment.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Warden);
            Log.Message("Socialite!");

            // Builder
            WorkSpecification builder = new WorkSpecification();
            builder.Name = "Builder";
            builder.TargetWorkers = new IntPawnAmount() { Value = 2 };
            builder.IsCritical = true;
            builder.Commitment = 0.5f;
            builder.Fitness.Add(new LearnRatePawnFitness());
            builder.Fitness.Add(new SkillLevelPawnFitness());
            builder.PostProcessors.Add(new SetTitlePawnPostProcessor());
            builder.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Construction);
            builder.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Construction);
            builder.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Construction);
            builder.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Construction);
            Log.Message("Builder!");

            // Farmhand
            WorkSpecification farmhand = new WorkSpecification();
            farmhand.Name = "Farmhand";
            farmhand.TargetWorkers = new PercentagePawnAmount() { Percentage = 0.2f };
            farmhand.IsCritical = true;
            farmhand.Commitment = 1f;
            farmhand.Fitness.Add(new LearnRatePawnFitness());
            farmhand.Fitness.Add(new SkillLevelPawnFitness());
            farmhand.PostProcessors.Add(new SetTitlePawnPostProcessor());
            farmhand.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Growing);
            farmhand.Priorities.OrderedPriorities.Add(WorkTypeDefOf.PlantCutting);
            farmhand.Priorities.OrderedPriorities.Add(WorkTypeDefOf.PlantCutting);
            farmhand.Priorities.OrderedPriorities.Add(WorkTypeDefOf.PlantCutting);
            farmhand.Priorities.OrderedPriorities.Add(WorkTypeDefOf.PlantCutting);
            Log.Message("Farmhand!");

            // Artisan
            WorkSpecification artisan = new WorkSpecification();
            artisan.Name = "Artisan";
            artisan.TargetWorkers = new IntPawnAmount() { Value = 3 };
            artisan.Commitment = 1f;
            artisan.Fitness.Add(new LearnRatePawnFitness());
            artisan.Fitness.Add(new SkillLevelPawnFitness());
            artisan.PostProcessors.Add(new SetTitlePawnPostProcessor());
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Smithing);
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Crafting);
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Crafting);
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Crafting);
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Crafting);
            artisan.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Crafting);
            Log.Message("Artisan!");

            // Miner
            WorkSpecification miner = new WorkSpecification();
            miner.Name = "Miner";
            miner.TargetWorkers = new IntPawnAmount() { Value = 1 };
            miner.Commitment = 0.5f;
            miner.Fitness.Add(new LearnRatePawnFitness());
            miner.Fitness.Add(new SkillLevelPawnFitness());
            miner.PostProcessors.Add(new SetTitlePawnPostProcessor());
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            miner.Priorities.OrderedPriorities.Add(WorkTypeDefOf.Mining);
            Log.Message("Miner!");

            return new List<WorkSpecification>() {
                doctorAssignment,
                researcher,
                socialAssignment,
                farmhand,
                builder,
                miner,
                artisan
            };
        }
    }
}