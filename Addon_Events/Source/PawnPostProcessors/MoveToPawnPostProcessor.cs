using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class MoveToPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public IntVec3? MoveToPosition;
        public IPawnPostProcessor OnArrived;

        private Buffer<Coroutine> _activeCoroutines = new Buffer<Coroutine>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && MoveToPosition.HasValue)
            {
                IntVec3 moveTo = MoveToPosition.Value;

                if (!pawn.drafter.Drafted)
                    pawn.drafter.Drafted = true;

                IntVec3 standableCell = CellFinder.StandableCellNear(moveTo, request.Map, 3);
                if (standableCell.IsValid)
                {
                    MultiPawnGotoController controller = Find.Selector.gotoController;
                    controller.StartInteraction(moveTo);
                    controller.AddPawn(pawn);
                    controller.FinalizeInteraction();

                    Coroutine current = _activeCoroutines.Get(pawn);
                    if (current != null) 
                        Find.Root.StopCoroutine(current);

                    Coroutine coroutine = Find.Root.StartCoroutine(ExecuteOnGotoFinished(pawn, () => OnArrived?.PostProcess(pawn, workSpecification, request)));
                    _activeCoroutines.Set(pawn, coroutine);
                }
            }
        }

        private IEnumerator ExecuteOnGotoFinished(Pawn pawn, Action action)
        {
            bool isGoto = pawn.CurJobDef == JobDefOf.Goto;
            if (isGoto)
            {
                bool arrived = false;
                while (!arrived)
                {
                    arrived = Vector3.SqrMagnitude(pawn.CurJob.targetA.CenterVector3 - pawn.Position.ToVector3()) < 1;
                    if (arrived)
                    {
                        action();
                        break;
                    }
                    yield return null;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MoveToPosition, "moveToPosition");
            Scribe_Deep.Look(ref OnArrived, "onArrived");
        }
    }
}
