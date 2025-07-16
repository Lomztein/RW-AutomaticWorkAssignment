using AutomaticWorkAssignment;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class NeedPawnFitness : PawnSetting, IPawnFitness
    {
        public NeedDef NeedDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && NeedDef != null)
            {
                Need need = pawn.needs.TryGetNeed(NeedDef);
                if (need != null)
                {
                    return need.CurLevel;
                }
            }
            return 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref NeedDef, "needDef");
        }
    }
}
