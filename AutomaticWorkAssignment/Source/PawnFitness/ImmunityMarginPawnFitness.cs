using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ImmunityMarginPawnFitness : PawnSetting, IPawnFitness
    {
        public HediffDef Hediff;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.health != null)
            {
                if (Hediff == null)
                {
                    var immunizable = GetImmunizableHediffs(pawn.health.hediffSet);
                    if (immunizable.Any())
                        return immunizable.Min(x => x.Item2.Immunity - x.Item1.Severity);

                    return 1f;
                }
                else
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Hediff);
                    if (hediff != null)
                        return pawn.health.immunity.GetImmunity(Hediff) - hediff.Severity;

                    return 1f;
                }
            }
            return 1f;
        }

        private IEnumerable<Tuple<Hediff, HediffComp_Immunizable>> GetImmunizableHediffs (HediffSet set)
        {
            foreach (var hediff in set.hediffs)
            {
                if (hediff.def != null && IsImmunizable(hediff.def))
                    yield return Tuple.Create(hediff, hediff.TryGetComp<HediffComp_Immunizable>());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref Hediff, "hediff");
        }

        public static IEnumerable<HediffDef> GetApplicableHediffDefs()
            => DefDatabase<HediffDef>.AllDefs.Where(x => (IsImmunizable(x) || IsImmunizable(x)) && x.debugLabelExtra != "animal");

        private static bool IsImmunizable(HediffDef def)
            => def.HasComp(typeof(HediffComp_Immunizable)) && def.CompProps<HediffCompProperties_Immunizable>()?.immunityPerDaySick > 0;
    }
}
