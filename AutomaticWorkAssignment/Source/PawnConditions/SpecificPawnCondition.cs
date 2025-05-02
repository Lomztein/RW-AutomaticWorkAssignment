using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SpecificPawnCondition : PawnSetting, IPawnCondition
    {
        public Pawn Pawn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Pawn, "pawn");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn == Pawn;
    }
}
