﻿using Lomzie.AutomaticWorkAssignment;
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
                if (workSpec.IsCritical && IsBelowMinimalAfter(workSpec, -1))
                {
                    var pawnsAssignedTo = WorkManager.Instance.GetPawnsAssignedTo(workSpec);
                    foreach (var pawn in pawnsAssignedTo)
                    {
                        WorkAssignment critical = WorkManager.Instance.GetAssignmentTo(pawn, workSpec);
                        if (!WorkManager.Instance.CanBeAssignedNow(pawn) && !critical.IsSubstituted)
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
            int numAssigned = WorkManager.Instance.GetCountAssignedTo(spec);
            numAssigned += change;
            int target = spec.MinWorkers.GetCount();
            return numAssigned < target;
        }

        private Pawn FindSubstituteFor(WorkAssignment critical)
        {
            ResolveWorkRequest request = WorkManager.Instance.MakeDefaultRequest();
            return critical.Specification.GetApplicableOrMinimalPawnsSorted(request.Pawns, request)
                .Where(x => !WorkManager.Instance.IsAssignedTo(x, critical.Specification)).FirstOrDefault();
        }

        private IEnumerator MakeSubstitution(Pawn original, Pawn substitute, WorkAssignment forAssignment)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                WorkAssignment substituteAssignment = WorkManager.Instance.AssignWorkToPawn(forAssignment.Specification, substitute, forAssignment.Index);
                forAssignment.SubstituteWith(substituteAssignment);
                WorkManager.Instance.RemoveAssignmentFromPawn(forAssignment, original);
                WorkManager.Instance.ResolvePawnPriorities(substitute);
            }catch(Exception ex)
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
