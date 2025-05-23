﻿using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PartHediffPawnCondition : PawnSetting, IPawnCondition
    {
        public HediffDef HediffDef;
        public BodyPartRecord HediffPart
        {
            get { return _bodyPartIndex > 0 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }
        private int _bodyPartIndex;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref HediffDef, "hediffDef");
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn?.health.hediffSet?.hediffs.Any(x => x.def == HediffDef && x.Part.Index == _bodyPartIndex) ?? false;
    }
}
