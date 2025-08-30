using RimWorld;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkAssignmentTracker : MapComponent
    {
        private int _checkDelayTicks = 60;
        private MapWorkManager _workManager;

        public WorkAssignmentTracker(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (GenTicks.TicksGame % _checkDelayTicks == 0)
            {
                _workManager ??= MapWorkManager.GetManager(map);
                CheckCriticalAssignments();
            }
        }

        private void CheckCriticalAssignments()
        {
            CheckCriticalAssignmentsCoroutine();
        }

        private void CheckCriticalAssignmentsCoroutine()
        {
            foreach (WorkSpecification workSpec in _workManager.WorkList)
            {
                if (workSpec.IsCritical && IsBelowMinimalAfter(workSpec, -1))
                {
                    var pawnsAssignedTo = _workManager.GetPawnsAssignedTo(workSpec);
                    foreach (var pawn in pawnsAssignedTo)
                    {
                        WorkAssignment critical = _workManager.GetAssignmentTo(pawn, workSpec);
                        if (!_workManager.CanBeAssignedNow(pawn) && !critical.IsSubstituted)
                        {
                            Pawn substitute = FindSubstituteFor(critical);
                            if (substitute != null)
                            {
                                Find.Root.StartCoroutine(MakeSubstitution(pawn, substitute, critical));
                                DoSubstituteFoundMessage(pawn, substitute, critical.Specification);
                            }
                        }
                    }
                }
            }
        }

        private bool IsBelowMinimalAfter(WorkSpecification spec, int change)
        {
            int numAssigned = _workManager.GetCountAssignedTo(spec);
            numAssigned += change;
            int target = spec.MinWorkers.GetCount(spec, _workManager.MakeDefaultRequest());
            return numAssigned < target;
        }

        private Pawn FindSubstituteFor(WorkAssignment critical)
        {
            ResolveWorkRequest request = _workManager.MakeDefaultRequest();
            return critical.Specification.GetApplicableOrMinimalPawnsSorted(request.Pawns, request)
                .Where(x => !_workManager.IsAssignedTo(x, critical.Specification)).FirstOrDefault();
        }

        private IEnumerator MakeSubstitution(Pawn original, Pawn substitute, WorkAssignment forAssignment)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                WorkAssignment substituteAssignment = _workManager.AssignWorkToPawn(forAssignment.Specification, substitute, forAssignment.Index);
                forAssignment.SubstituteWith(substituteAssignment);
                _workManager.RemoveAssignmentFromPawn(forAssignment, original);
                _workManager.ResolvePawnPriorities(substitute);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
        }

        private void DoSubstituteFoundMessage(Pawn original, Pawn substitute, WorkSpecification workSpec)
        {
            LookTargets targets = new LookTargets(original, substitute);

            Message message = new Message(
                "AWA.SubstituteMessage".Translate().Formatted(original, workSpec.Name, substitute),
                MessageTypeDefOf.NegativeEvent, targets);
            Messages.Message(message);
        }
    }
}
