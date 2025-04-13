using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DevelopmentalStagePawnCondition : IPawnCondition
    {
        public string Label => "Developmental stage";
        public string Description => "Check if the pawn is a given developmental stage.";

        public DevelopmentalStage DevelopmentalStage;

        public void ExposeData()
        {
            Scribe_Values.Look(ref DevelopmentalStage, "developmentalStage");
        }

        public DevelopmentalStagePawnCondition()
        {
            DevelopmentalStage = DevelopmentalStage.Adult;
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.ageTracker?.CurLifeStage.developmentalStage == DevelopmentalStage;
    }
}
