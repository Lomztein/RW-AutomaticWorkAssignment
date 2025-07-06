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
            if (BillRecipeDef == null)
            {
                Log.Warning($"[AWA:AS] BillRecipeDef is null for work specification '{workSpecification.Name}'");
                return;
            }

            // Get the correct body part for this pawn
            BodyPartRecord bodyPart = GetRecordOnPawn(pawn, BodyPartRecord);

            // Create the medical bill
            Bill_Medical bill = new Bill_Medical(BillRecipeDef, new List<Thing>()) { Part = bodyPart };

            // Check if we can apply this bill to the pawn
            if (CanApplyTo(pawn, bill))
            {
                // Try to reserve ingredients - only add the bill if ingredients are available
                if (TryReserve(bill, pawn.Map))
                {
                    // Add the bill to the pawn's bill stack
                    pawn.BillStack.AddBill(bill);
                }
                else
                {
                    LogError($"Failed to reserve ingredients for bill '{bill.recipe.LabelCap}'");
                }
            }
            else
            {
                LogError($"Automatic surgery could not apply to pawn '{pawn}'");
            }
        }

        BodyPartRecord GetRecordOnPawn(Pawn pawn, BodyPartRecord record)
        {
            if (record == null)
                return null;
                
            try
            {
                var parts = pawn.RaceProps.body.GetPartsWithDef(record.def);
                if (parts != null && parts.Count > 0)
                {
                    // Try to find exact match only
                    var exactMatch = parts.Find(x => x.LabelCap == record.LabelCap);
                    return exactMatch;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[AWA:AS] Error getting body part for {pawn.NameShortColored}: {ex.Message}");
            }
            
            return null;
        }

        private bool TryReserve(Bill_Medical bill, Map onMap)
        {
            if (onMap == null)
                return false;
                
            try
            {
                List<Tuple<Thing, int>> reservables = new List<Tuple<Thing, int>>();
                foreach (var ingredient in bill.recipe.ingredients)
                {
                    foreach (var def in ingredient.filter.AllowedThingDefs)
                    {
                        if (!IsMedicine(def))
                        {
                            int count = ingredient.CountRequiredOfFor(def, bill.recipe, bill);
                            Thing reservable = MapWorkManager.GetManager(onMap).Reservations.FindReservable(def, count, onMap);
                            if (reservable != null)
                            {
                                reservables.Add(new Tuple<Thing, int>(reservable, count));
                            }
                            else
                            {
                                return LogError($"Failed to find {count} reservable: '{def.LabelCap}'");
                            }
                        }
                    }
                }

                foreach (var reservable in reservables)
                {
                    MapWorkManager.GetManager(onMap).Reservations.Reserve(reservable.Item1, reservable.Item2);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error reserving ingredients: {ex.Message}");
                return false;
            }
        }

        private bool IsMedicine(ThingDef def)
            => def.IsMedicine || def.thingCategories.Contains(ThingCategoryDefOf.Medicine);

        private bool CanApplyTo(Pawn pawn, Bill_Medical bill)
        {
            if (BillRecipeDef == null || bill == null)
                return LogError("Bill recipe def or bill was null");
                
            if (pawn == null)
                return LogError("Pawn is null");

            // Check if selected body part is compatable. Commented out because it may have caused issues.
            //if (bill.Part != null && !BillRecipeDef.Worker.GetPartsToApplyOn(pawn, BillRecipeDef).Any(x => x.LabelCap == bill.Part.LabelCap))
            //    return LogError($"Target body part mismatch: {bill.Part?.LabelCap ?? "null"}");

            // Check if recipe is available on the pawn
            if (!BillRecipeDef.Worker.AvailableOnNow(pawn, bill.Part))
                return LogError($"Recipe worker {BillRecipeDef.Worker.GetType().Name} AvailableOnNow({pawn}, {bill.Part?.LabelCap}) = false");
                
            // Check if pawn already has the resulting hediff
            if (BillRecipeDef.addsHediff != null && HasHediff(pawn, BillRecipeDef.addsHediff, bill.Part))
                return LogError($"Target pawn '{pawn}' already has resulting hediff '{BillRecipeDef.addsHediff}'.");
                
            // Check if recipe requires fixed body parts but we don't have one
            if (BillRecipeDef.appliedOnFixedBodyParts.Any() && bill.Part == null)
                return LogError($"Recipe applied on fixed body parts, but body part record = null.");
                
            // Check if identical bill already exists
            if (pawn.BillStack.Bills.Where(x => x is Bill_Medical).Cast<Bill_Medical>()
                .Any(x => x.recipe == bill.recipe && x.Part == bill.Part))
                return LogError($"Identical bill already present on pawn '{pawn}'.");

            return true;
        }

        private bool HasHediff (Pawn pawn, HediffDef hediff, BodyPartRecord bodyPartRecord)
        {
            if (bodyPartRecord == null)
                return pawn.health.hediffSet.HasHediff(hediff);
            return pawn?.health.hediffSet?.hediffs.Any(x => 
                x.def == hediff && 
                x.Part != null && 
                x.Part.LabelCap == bodyPartRecord.LabelCap) ?? false;
        }

        private bool IsOnMap(RecipeDef recipe, IngredientCount ingredientCount, Map map)
        {
            if (map == null)
                return false;
                
            return map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                .Any(x => ingredientCount.filter.Allows(x) && ingredientCount.CountFor(recipe) <= x.stackCount - MapWorkManager.GetManager(map).Reservations.Get(x));
        }

        private bool LogError(string message)
        {
            Logger.Message("[AWA:AS] AddBillPostProcessor failed: " + message);
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
