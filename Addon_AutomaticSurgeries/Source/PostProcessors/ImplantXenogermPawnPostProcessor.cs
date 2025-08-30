using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class ImplantXenogermPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public string XenogermLabel;

        private RecipeDef ImplantDef => RecipeDefOf.ImplantXenogerm;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (string.IsNullOrEmpty(XenogermLabel))
            {
                Log.Warning($"[AWA:AS] No xenogerm set for ImplantXenogermPostProcessor in work specification '{workSpecification.Name}'");
                return;
            }

            // Create the medical bill
            Xenogerm xenogerm = FindXenogerm(XenogermLabel, request);
            if (xenogerm == null)
            {
                BillUtils.LogError($"Failed to find xenogerm on map with label {XenogermLabel}");
                return;
            }

            if (pawn.genes?.xenotypeName?.ToLower() != XenogermLabel.ToLower())
            {
                request.WorkManager.Reservations.Reserve(xenogerm, 1);
                Bill_Medical bill = new Bill_Medical(ImplantDef, new List<Thing>()) { xenogerm = xenogerm };

                // Check if we can apply this bill to the pawn
                if (BillUtils.CanApplyTo(pawn, bill))
                {
                    // Try to reserve ingredients - only add the bill if ingredients are available
                    if (BillUtils.TryReserve(bill, pawn.Map))
                    {
                        // Add the bill to the pawn's bill stack
                        pawn.BillStack.AddBill(bill);
                    }
                    else
                    {
                        BillUtils.LogError($"Failed to reserve ingredients for bill '{bill.recipe.LabelCap}'");
                    }
                }
                else
                {
                    BillUtils.LogError($"Automatic surgery could not apply to pawn '{pawn}'");
                }
            }
        }

        private Xenogerm FindXenogerm(string xenogermName, ResolveWorkRequest request)
            => request.WorkManager.Reservations.FindUnreserved(request.Map, x => x is Xenogerm xenogerm && xenogerm.xenotypeName.ToLower() == xenogermName.ToLower(), 1) as Xenogerm;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref XenogermLabel, "xenogermLabel");
        }

        public static IEnumerable<string> GetValidXenogermLabels()
            => Find.CustomXenogermDatabase.CustomXenogermsForReading.Select(x => x.name).Concat(DefDatabase<XenotypeDef>.AllDefs.Select(x => x.label));

        public static IEnumerable<BodyPartRecord> GetValidBodyPartsFor(RecipeDef recipeDef)
            => BodyDefOf.Human.AllParts.Where(x => recipeDef.appliedOnFixedBodyParts.Any(y => x.def == y));
    }
}
