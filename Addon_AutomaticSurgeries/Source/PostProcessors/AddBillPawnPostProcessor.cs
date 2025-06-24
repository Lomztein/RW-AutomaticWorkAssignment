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
            get { return _bodyPartIndex != -1 ? BodyDefOf.Human.GetPartAtIndex(_bodyPartIndex) : null; }
            set { _bodyPartIndex = BodyDefOf.Human.GetIndexOfPart(value); }
        }

        private int _bodyPartIndex = -1;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (BillRecipeDef != null)
            {
                BodyPartRecord bodyPart = GetRecordOnPawn(pawn, BodyPartRecord);
                if (bodyPart != null || BodyPartRecord == null)
                {
                    Bill_Medical bill = new Bill_Medical(BillRecipeDef, null) { Part = bodyPart };
                    if (CanApplyTo(pawn, bill) && TryReserve(bill, pawn.Map))
                    {
                        pawn.BillStack.AddBill(bill);
                    }
                }
            }
        }

        BodyPartRecord GetRecordOnPawn(Pawn pawn, BodyPartRecord record)
        {
            if (record != null)
            {
                var parts = pawn.RaceProps.body.GetPartsWithDef(record.def);
                return parts.Find(x => x.LabelCap == record.LabelCap);
            }
            return null;
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
                return LogError("Bill recipe def or bill was null");
            if (!BillRecipeDef.Worker.AvailableOnNow(pawn, BodyPartRecord))
                return LogError($"Recipe worker {BillRecipeDef.Worker.GetType().Name} AvailableOnNow({pawn}, {BodyPartRecord?.LabelCap}) = false");
            //if (BodyPartRecord != null && !BillRecipeDef.Worker.GetPartsToApplyOn(pawn, BillRecipeDef).Any(x => x.LabelCap == BodyPartRecord.LabelCap))
            //    return LogError("Target body part mismatch.");
            if (BillRecipeDef.addsHediff != null && HasHediff(pawn, BillRecipeDef.addsHediff, bill.Part))
                return LogError($"Target pawn '{pawn}' already has resulting hediff '{BillRecipeDef.addsHediff}'.");
            if (BillRecipeDef.appliedOnFixedBodyParts.Any() && BodyPartRecord == null)
                return LogError($"Recipe applied on fixed body parts, but body part record = null.");
            if (pawn.BillStack.Bills.Where(x => x is Bill_Medical).Cast<Bill_Medical>()
                .Any(x => x.recipe == bill.recipe && x.Part == bill.Part))
                return LogError($"Identical bill already present on pawn '{pawn}'.");
            if (BillRecipeDef.ingredients.Any(x => !IsOnMap(BillRecipeDef, x, pawn.Map)))
                return LogError($"Missing or reserved ingredients on map.");
            return true;
        }

        private bool HasHediff (Pawn pawn, HediffDef hediff, BodyPartRecord bodyPartRecord)
        {
            if (bodyPartRecord == null)
                return pawn.health.hediffSet.HasHediff(BillRecipeDef.addsHediff);
            return pawn?.health.hediffSet?.hediffs.Any(x => x.def == hediff && x.Part.LabelCap == bodyPartRecord.LabelCap) ?? false;
        }

        private bool IsOnMap(RecipeDef recipe, IngredientCount ingredientCount, Map map)
        {
            return map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                .Any(x => ingredientCount.filter.Allows(x) && ingredientCount.CountFor(recipe) <= x.stackCount - WorkManager.Instance.Reservations.Get(x));
        }

        private bool LogError(string message)
        {
            Log.Message("[AWA:AS] AddBillPostProcessor failed: " + message);
            return false;
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
