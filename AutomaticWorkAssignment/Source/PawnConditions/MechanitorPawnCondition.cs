using RimWorld;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class MechanitorPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.health != null)
            {
                return pawn.health.hediffSet.HasHediff<Hediff_Mechlink>();                
            }
            return false;
        }
    }
}
