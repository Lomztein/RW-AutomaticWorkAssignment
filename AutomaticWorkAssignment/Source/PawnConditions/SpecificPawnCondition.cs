using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SpecificPawnCondition : IPawnCondition
    {
        public string Label => "Is pawn";
        public string Description => "Check if the pawn is a specific pawn.";

        public Pawn Pawn;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "pawn");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn == Pawn;
    }
}
