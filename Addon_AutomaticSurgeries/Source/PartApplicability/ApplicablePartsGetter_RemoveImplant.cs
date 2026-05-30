using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PartApplicability
{
    internal class ApplicablePartsGetter_RemoveImplant : ApplicablePartsGetter<Recipe_RemoveImplant>
    {
        protected override IEnumerable<BodyPartRecord> GetApplicablePartsFor(Recipe_RemoveImplant worker)
            => DefDatabase<RecipeDef>.AllDefs.Where(x => x.addsHediff != null && x.addsHediff == worker.recipe.removesHediff).SelectMany(x => BillUtils.GetFixedPartsToEverApplyOn(x));
    }
}
