using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class MechanoidCountPawnFitness : PawnSetting, IPawnFitness
    {
        public PawnKindDef MechanoidDef;
        
        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.mechanitor != null)
            {
                return GetAmount(pawn.mechanitor, MechanoidDef);
            }
            return 0;
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
