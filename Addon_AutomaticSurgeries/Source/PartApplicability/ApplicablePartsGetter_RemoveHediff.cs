using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PartApplicability
{
    internal class ApplicablePartsGetter_RemoveHediff : ApplicablePartsGetter<Recipe_RemoveHediff>
    {
        protected override IEnumerable<BodyPartRecord> GetApplicablePartsFor(Recipe_RemoveHediff worker)
        {
            List<BodyPartRecord> parts = new List<BodyPartRecord>();
            // Is the hediff given by a recipe?
            parts.AddRange(DefDatabase<RecipeDef>.AllDefs.Where(x => x.addsHediff != null && x.addsHediff == worker.recipe.removesHediff).SelectMany(x => BillUtils.GetFixedPartsToEverApplyOn(x)));

            // Is the hediff given by a HediffGiver?
            IEnumerable<HediffGiver> givers = DefDatabase<HediffGiverSetDef>.AllDefs.SelectMany(x => x.hediffGivers.Where(y => y.hediff == worker.recipe.removesHediff));
            foreach (var giver in givers)
            {
                if (giver.canAffectAnyLivePart)
                {
                    return BillUtils.GetAllParts();
                }
                else
                {
                    parts.AddRange(giver.partsToAffect.Select(x => BillUtils.GetAllParts().First(y => x == y.def)));
                }
            }

            return parts.Distinct();
        }
    }
}
