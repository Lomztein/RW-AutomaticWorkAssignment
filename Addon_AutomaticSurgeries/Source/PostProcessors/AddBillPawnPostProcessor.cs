using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class AddBillPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public RecipeDef BillRecipeDef;
        public BodyPartRecord BodyPartRecord 
        {
            get { return _bodyPartIndex > 0 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }

        private int _bodyPartIndex;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (BillRecipeDef != null)
            {
                Bill_Medical bill = new Bill_Medical(BillRecipeDef, null) { Part = BodyPartRecord };
                if (CanApplyTo(pawn, bill) && TryReserve(bill, pawn.Map))
                {
                    pawn.BillStack.AddBill(bill);
                }
            }
        }

        private bool TryReserve(Bill_Medical bill, Map onMap)
        {
            List<Tuple<Thing, int>> reservables = new List<Tuple<Thing, int>>();
            foreach (var ingredient in bill.recipe.ingredients)
            {
                foreach (var def in ingredient.filter.AllowedThingDefs)
                {
                    if (!def.IsMedicine)
                    {
                        int count = ingredient.CountRequiredOfFor(def, bill.recipe, bill);
                        Thing reservable = WorkManager.Instance.Reservations.FindReservable(def, count, onMap);
                        if (reservable != null)
                        {
                            reservables.Add(new Tuple<Thing, int>(reservable, count));
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            foreach (var reservable in reservables)
            {
                WorkManager.Instance.Reservations.Reserve(reservable.Item1, reservable.Item2);
            }

            return true;
        }

        private bool CanApplyTo(Pawn pawn, Bill_Medical bill)
        {
            if (BillRecipeDef == null || bill == null)
                return false;
            if (!BillRecipeDef.Worker.AvailableOnNow(pawn, BodyPartRecord))
                return false;
            if (BodyPartRecord != null && !BillRecipeDef.Worker.GetPartsToApplyOn(pawn, BillRecipeDef).Any(x => x.Index == BodyPartRecord.Index))
                return false;
            if (BillRecipeDef.addsHediff != null && pawn.health.hediffSet.HasHediff(BillRecipeDef.addsHediff))
                return false;
            if (BillRecipeDef.appliedOnFixedBodyParts.Any() && BodyPartRecord == null)
                return false;
            if (pawn.BillStack.Bills.Where(x => x is Bill_Medical).Cast<Bill_Medical>()
                .Any(x => x.recipe == bill.recipe && x.Part == bill.Part))
                return false;
            if (BillRecipeDef.ingredients.Any(x => !IsOnMap(BillRecipeDef, x, pawn.Map)))
                return false;
            return true;
        }

        private bool IsOnMap(RecipeDef recipe, IngredientCount ingredientCount, Map map)
        {
            return map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                .Any(x => ingredientCount.filter.Allows(x) && ingredientCount.CountFor(recipe) <= x.stackCount - WorkManager.Instance.Reservations.Get(x));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref BillRecipeDef, "billRecipeDef");
            Scribe_Values.Look(ref _bodyPartIndex, "bodyPartIndex");
        }

        public static IEnumerable<RecipeDef> GetValidRecipies()
            => DefDatabase<RecipeDef>.AllDefs.Where(x => x.IsSurgery);

        public static IEnumerable<BodyPartRecord> GetValidBodyPartsFor(RecipeDef recipeDef)
            => BodyDefOf.Human.AllParts.Where(x => recipeDef.appliedOnFixedBodyParts.Any(y => x.def == y));
    }
}
