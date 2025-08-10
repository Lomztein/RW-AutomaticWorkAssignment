using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetFoodPolicyPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public FoodPolicy Policy;
        private string _policyName;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Policy, "policy");

            if (Scribe.mode == LoadSaveMode.Saving && Policy != null)
                _policyName = Policy.InspectLabel;

            Scribe_Values.Look(ref _policyName, "policyName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Policy == null && _policyName != null)
                Policy = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Find(x => x.InspectLabel == _policyName);
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (Policy != null)
            {
                pawn.foodRestriction.CurrentFoodPolicy = Policy;
            }
        }
    }
}
