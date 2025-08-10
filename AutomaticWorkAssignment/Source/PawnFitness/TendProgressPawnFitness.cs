using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class TendProgressPawnFitness : PawnSetting, IPawnFitness
    {
        public HediffDef Hediff;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.health != null)
            {
                if (Hediff == null)
                {
                    var tendable = GetClearsAfterTendHediffs(pawn.health.hediffSet);
                    if (tendable.Any())
                        return tendable.Min(x => GetTotalTendQuality(x.Item2) / x.Item2.TProps.disappearsAtTotalTendQuality);

                    return 1f;
                }
                else
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Hediff);
                    Log.Message(hediff?.def.defName ?? "null");
                    HediffComp_TendDuration tendDuration = hediff.TryGetComp<HediffComp_TendDuration>();
                    if (hediff != null)
                        return GetTotalTendQuality(tendDuration) / tendDuration.TProps.disappearsAtTotalTendQuality;

                    return 1f;
                }
            }
            return 1f;
        }

        private IEnumerable<Tuple<Hediff, HediffComp_TendDuration>> GetClearsAfterTendHediffs(HediffSet set)
        {
            foreach (var hediff in set.hediffs)
            {
                if (hediff.def != null && ClearsAfterTend(hediff.def))
                    yield return Tuple.Create(hediff, hediff.TryGetComp<HediffComp_TendDuration>());
            }
        }

        private FieldInfo _totalQualityField;
        private float GetTotalTendQuality(HediffComp_TendDuration tendDuration)
        {
            if (_totalQualityField == null)
                _totalQualityField = tendDuration.GetType().GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);

            return (float)_totalQualityField.GetValue(tendDuration);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref Hediff, "hediff");
        }

        public static IEnumerable<HediffDef> GetApplicableHediffDefs()
            => DefDatabase<HediffDef>.AllDefs.Where(x => ClearsAfterTend(x) && x.debugLabelExtra != "animal");

        private static bool ClearsAfterTend(HediffDef def)
            => def.HasComp(typeof(HediffComp_TendDuration)) && def.CompProps<HediffCompProperties_TendDuration>()?.disappearsAtTotalTendQuality != -1;
    }
}
