using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class IdeoPreceptPawnCondition : PawnSetting, IPawnCondition
    {
        public PreceptDef PreceptDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Ideo != null)
            {
                var precept = pawn.Ideo.PreceptsListForReading.FirstOrDefault(x => x.def == PreceptDef);
                if (precept != null)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref PreceptDef, "preceptDef");
        }
    }
}
