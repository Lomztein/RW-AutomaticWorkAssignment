using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetIdeoRolePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public PreceptDef RoleDef;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Ideo != null)
            {
                Precept_Role curRolePrecept = pawn.Ideo.PreceptsListForReading.Find(x => x is Precept_Role pr && pr.ChosenPawns().Contains(pawn)) as Precept_Role;
                if (curRolePrecept != null && curRolePrecept.def != RoleDef)
                {
                    curRolePrecept.Unassign(pawn, true);
                }

                Precept precept = pawn.Ideo.PreceptsListForReading.FirstOrDefault(x => x.def == RoleDef);
                if (precept != null && precept is Precept_Role rolePrecept)
                {
                    rolePrecept.Assign(pawn, true);
                }
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Defs.Look(ref RoleDef, "roleDef");
        }
    }
}
