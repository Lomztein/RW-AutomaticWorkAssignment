using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class HealthConditionSeverityPawnFitness : PawnSetting, IPawnFitness
    {
        public HediffDef Hediff;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.health != null)
            {
                if (Hediff == null)
                {
                    return pawn.health.hediffSet.hediffs.Max(x => x.Severity);
                }
                else
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Hediff);
                    if (hediff != null)
                        return hediff.Severity;

                    return 0f;
                }
            }
            return 0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref Hediff, "hediff");
        }
        public static IEnumerable<HediffDef> GetApplicableHediffDefs()
            => DefDatabase<HediffDef>.AllDefs.Where(x => x.maxSeverity != 0);
    }
}
