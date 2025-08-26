using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class UseAbilityPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public AbilityDef AbilityDef;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.abilities != null && AbilityDef != null)
            {
                Ability ability = pawn.abilities.abilities.FirstOrDefault(x => x.def == AbilityDef);
                LocalTargetInfo pawnTarget = new LocalTargetInfo(pawn);
                LocalTargetInfo positionTarget = new LocalTargetInfo(pawn.Position);
                LocalTargetInfo? finalTarget = null;
                if (ability.CanApplyOn(pawnTarget))
                {
                    finalTarget = pawnTarget;
                }
                if (ability.CanApplyOn(positionTarget))
                {
                    finalTarget = positionTarget;
                }

                if (finalTarget.HasValue && finalTarget.Value.IsValid && pawn.jobs.curJob?.ability != ability)
                {
                    pawn.jobs.StartJob(ability.GetJob(finalTarget.Value, finalTarget.Value));
                }
            }
        }
    }
}
