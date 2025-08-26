using RimWorld;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class MechanoidsPawnCondition : PawnSetting, IPawnCondition
    {
        public PawnKindDef MechanoidDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.mechanitor != null)
            {
                return GetAmount(pawn.mechanitor, MechanoidDef) > 0;
            }
            return false;
        }

        private int GetAmount(Pawn_MechanitorTracker tracker, PawnKindDef mechanoidDef)
        {
            if (MechanoidDef == null)
                return tracker.OverseenPawns.Count;
            return tracker.OverseenPawns.Count(x => x.kindDef == mechanoidDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref MechanoidDef, "mechanoidDef");
        }
    }
}
