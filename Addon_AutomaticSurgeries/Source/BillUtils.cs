using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class BillUtils
    {
        public static BodyPartRecord GetRecordOnPawn(Pawn pawn, BodyPartRecord record)
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

        public static bool TryReserve(Bill_Medical bill, Map onMap)
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

        public static bool IsMedicine(ThingDef def)
            => def.IsMedicine || def.thingCategories.Contains(ThingCategoryDefOf.Medicine);

        public static bool CanApplyTo(Pawn pawn, Bill_Medical bill)
        {
            if (bill == null || bill.recipe == null)
                return LogError("Bill recipe def or bill was null");

            RecipeDef recipeDef = bill.recipe;

            if (pawn == null)
                return LogError("Pawn is null");

            // Check if selected body part is compatable. Commented out because it may have caused issues.
            //if (bill.Part != null && !BillRecipeDef.Worker.GetPartsToApplyOn(pawn, BillRecipeDef).Any(x => x.LabelCap == bill.Part.LabelCap))
            //    return LogError($"Target body part mismatch: {bill.Part?.LabelCap ?? "null"}");

            // Check if recipe is available on the pawn
            if (!recipeDef.Worker.AvailableOnNow(pawn, bill.Part))
                return LogError($"Recipe worker {recipeDef.Worker.GetType().Name} AvailableOnNow({pawn}, {bill.Part?.LabelCap}) = false");

            // Check if pawn already has the resulting hediff
            if (recipeDef.addsHediff != null && HasHediff(pawn, recipeDef.addsHediff, bill.Part))
                return LogError($"Target pawn '{pawn}' already has resulting hediff '{recipeDef.addsHediff}'.");

            // Check if recipe requires fixed body parts but we don't have one
            if (recipeDef.appliedOnFixedBodyParts.Any() && bill.Part == null)
                return LogError($"Recipe applied on fixed body parts, but body part record = null.");

            // Check if identical bill already exists
            if (pawn.BillStack.Bills.Where(x => x is Bill_Medical).Cast<Bill_Medical>()
                .Any(x => x.recipe == bill.recipe && x.Part == bill.Part))
                return LogError($"Identical bill already present on pawn '{pawn}'.");

            return true;
        }

        public static bool HasHediff(Pawn pawn, HediffDef hediff, BodyPartRecord bodyPartRecord)
        {
            if (bodyPartRecord == null)
                return pawn.health.hediffSet.HasHediff(hediff);
            return pawn?.health.hediffSet?.hediffs.Any(x =>
                x.def == hediff &&
                x.Part != null &&
                x.Part.LabelCap == bodyPartRecord.LabelCap) ?? false;
        }

        public static bool LogError(string message)
        {
            Logger.Message("[AWA:AS] AddBillPostProcessor failed: " + message);
            return false;
        }
    }

}
