using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class LifeStagePawnCondition : IPawnCondition
    {
        public string Label => "Life stage";
        public string Description => "Check if the pawn is a given life stage.";

        public LifeStageDef LifeStageDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref LifeStageDef, "lifeStage");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => LifeStageDef == null || pawn.ageTracker?.CurLifeStage == LifeStageDef;
    }
}
