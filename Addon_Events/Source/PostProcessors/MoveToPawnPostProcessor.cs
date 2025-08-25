using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class MoveToPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public IntVec3? MoveToPosition;

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
                    Find.Selector.gotoController.StartInteraction(moveTo);
                    Find.Selector.gotoController.AddPawn(pawn);
                    Find.Selector.gotoController.FinalizeInteraction();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MoveToPosition, "moveToPosition");
        }
    }
}
