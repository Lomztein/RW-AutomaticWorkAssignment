using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
