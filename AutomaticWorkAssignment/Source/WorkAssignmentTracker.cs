using Lomzie.AutomaticWorkAssignment;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkAssignmentTracker : GameComponent
    {
        public WorkAssignmentTracker(Game game) 
        {
        }

        private int _checkDelayTicks = 60;

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (GenTicks.TicksGame % _checkDelayTicks == 0)
            {
                CheckCriticalAssignments();
            }
        }

        private void CheckCriticalAssignments()
        {
            CheckCriticalAssignmentsCoroutine();
        }

        private void CheckCriticalAssignmentsCoroutine()
        {
            foreach (WorkSpecification workSpec in WorkManager.Instance.WorkList)
            {
                if (workSpec.IsCritical && WorkManager.Instance.IsWorkSpecificationMinimallySatisfied(workSpec))
                {
                    var pawnsAssignedTo = WorkManager.Instance.GetPawnsAssignedTo(workSpec);
                    foreach (var pawn in pawnsAssignedTo)
                    {
                        WorkAssignment critical = WorkManager.Instance.GetAssignmentTo(pawn, workSpec);
                        if (IsUnableToWork(pawn) && !critical.IsSubstituted)
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

        private bool IsUnableToWork(Pawn pawn)
        {
            return pawn != null && (pawn.InMentalState || pawn.DeadOrDowned);
        }

        private Pawn FindSubstituteFor(WorkAssignment critical)
        {
            ResolveWorkRequest request = WorkManager.Instance.MakeDefaultRequest();
            return critical.Specification.GetApplicablePawnsSorted(request.Pawns, request).FirstOrDefault();
        }

        private IEnumerator MakeSubstitution(Pawn original, Pawn substitute, WorkAssignment forAssignment)
        {
            yield return new WaitForEndOfFrame();
            WorkAssignment substituteAssignment = WorkManager.Instance.AssignWorkToPawn(forAssignment.Specification, substitute, forAssignment.Index);
            forAssignment.SubstituteWith(substituteAssignment);
            WorkManager.Instance.RemoveAssignmentFromPawn(forAssignment, original);
            WorkManager.Instance.ResolvePawnPriorities(substitute);
        }

        private void DoSubstituteFoundMessage(Pawn original, Pawn subtitute, WorkSpecification workSpec)
        {
            LookTargets targets = new LookTargets(original, subtitute);

            Message message = new Message(
                $"{original} has become unable to do their critical '{workSpec.Name}' work, and {subtitute} has been chosen to substitute.",
                MessageTypeDefOf.NegativeEvent, targets);
            Messages.Message(message);
        }
    }
}
