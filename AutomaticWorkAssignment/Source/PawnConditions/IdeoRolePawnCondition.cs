using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class IdeoRolePawnCondition : PawnSetting, IPawnCondition
    {
        public PreceptDef RoleDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Ideo != null)
            {
                if (RoleDef == null)
                    return pawn.Ideo.PreceptsListForReading.Any(x => x is Precept_Role preceptRole && preceptRole.IsAssigned(pawn));

                var precept = pawn.Ideo.PreceptsListForReading.FirstOrDefault(x => x.def == RoleDef);
                if (precept != null && precept is Precept_Role rolePrecept)
                {
                    return rolePrecept.IsAssigned(pawn);
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref RoleDef, "roleDef");
        }
    }
}
