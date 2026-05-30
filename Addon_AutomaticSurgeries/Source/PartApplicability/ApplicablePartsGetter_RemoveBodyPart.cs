using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PartApplicability
{
    internal class ApplicablePartsGetter_RemoveBodyPart : ApplicablePartsGetter<Recipe_RemoveBodyPart>
    {
        protected override IEnumerable<BodyPartRecord> GetApplicablePartsFor(Recipe_RemoveBodyPart worker)
        {
            return BillUtils.GetAllParts().Where(x => (x.def.canSuggestAmputation || x.def.forceAlwaysRemovable) && !x.IsCorePart);
        }
    }
}
