using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class SlavePawnCondition : IPawnCondition
    {
        public string Label => "Is slave";
        public string Description => "Check if the pawn is a slave of the colony.";

        public void ExposeData()
        {
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.IsSlaveOfColony;
    }
}
