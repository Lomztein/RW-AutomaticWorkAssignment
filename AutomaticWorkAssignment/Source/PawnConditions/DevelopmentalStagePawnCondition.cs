﻿using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DevelopmentalStagePawnCondition : PawnSetting, IPawnCondition
    {
        public DevelopmentalStage DevelopmentalStage;

        public override void ExposeData()
        {
            base.ExposeData();
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
