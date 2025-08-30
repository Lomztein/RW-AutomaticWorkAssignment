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
            BodyPartRecord bodyPart = BillUtils.GetRecordOnPawn(pawn, BodyPartRecord);

            // Create the medical bill
            Bill_Medical bill = new Bill_Medical(BillRecipeDef, new List<Thing>()) { Part = bodyPart };

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
