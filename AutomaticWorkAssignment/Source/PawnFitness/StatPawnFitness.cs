using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class StatPawnFitness : PawnSetting, IPawnFitness
    {
        public StatDef StatDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => StatDef != null ? pawn.GetStatValue(StatDef) : 0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref StatDef, "statDef");
        }
    }
}
