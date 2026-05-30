using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PartApplicability
{
    public abstract class ApplicablePartsGetter
    {
        private static List<ApplicablePartsGetter> _getters = new List<ApplicablePartsGetter>() {
            new ApplicablePartsGetter_RemoveBodyPart(),
            new ApplicablePartsGetter_RemoveHediff(),
            new ApplicablePartsGetter_RemoveImplant()
        };

        public static IEnumerable<BodyPartRecord> GetFor(RecipeDef def)
        {
            return _getters.FirstOrDefault(x => x.CanGetPartsFor(def))?.GetApplicablePartsFor(def) ?? Enumerable.Empty<BodyPartRecord>();
        }

        public abstract bool CanGetPartsFor(RecipeDef def);

        public IEnumerable<BodyPartRecord> GetApplicableParts(RecipeDef recipe)
        {
            if (recipe.targetsBodyPart)
            {
                return GetApplicablePartsFor(recipe);
            }
            return Enumerable.Empty<BodyPartRecord>();
        }

        protected abstract IEnumerable<BodyPartRecord> GetApplicablePartsFor(RecipeDef recipe);
    }

    public abstract class ApplicablePartsGetter<T> : ApplicablePartsGetter where T : RecipeWorker
    {
        public override bool CanGetPartsFor(RecipeDef def)
        {
            return def.Worker is T;
        }
        protected override IEnumerable<BodyPartRecord> GetApplicablePartsFor(RecipeDef recipe)
        {
            return GetApplicablePartsFor(recipe.Worker as T);
        }
        protected abstract IEnumerable<BodyPartRecord> GetApplicablePartsFor(T recipe);
    }
}
