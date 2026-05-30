using Lomzie.AutomaticWorkAssignment.PartApplicability;
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

            // If no body part is specified, try to find one automatically
            BodyPartRecord bodyPart = BodyPartRecord;
            if (BillRecipeDef.targetsBodyPart)
            {
                if (BodyPartRecord == null)
                {
                    IEnumerable<BodyPartRecord> validParts = GetValidBodyPartsFor(pawn);
                    bodyPart = validParts.FirstOrDefault(x => !BillUtils.HasBillForPart(pawn, BillRecipeDef, x));
                }
                // Get the correct body part for this pawn
                bodyPart = BillUtils.GetRecordOnPawn(pawn, bodyPart);
            }

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

        Cache<List<BodyPartRecord>> _availableOnCache = new Cache<List<BodyPartRecord>>();
        public IEnumerable<BodyPartRecord> GetValidBodyPartsForOnMap(Map map)
        {
            if (!_availableOnCache.TryGet(out List<BodyPartRecord> availableOn))
            {
                IEnumerable<Pawn> mapPawns = map.mapPawns.FreeColonistsAndPrisoners;
                List<BodyPartRecord> validParts = new List<BodyPartRecord>();
                foreach (Pawn pawn in mapPawns)
                {
                    IEnumerable<BodyPartRecord> parts = GetValidBodyPartsFor(pawn);
                    foreach (BodyPartRecord part in parts)
                    {
                        if (!validParts.Contains(part))
                        {
                            validParts.Add(part);
                        }
                    }

                }

                availableOn = validParts;
                _availableOnCache.Set(availableOn);
            }
            return BillUtils.GetFixedPartsToEverApplyOn(BillRecipeDef).Concat(availableOn).Distinct();
        }

        private IEnumerable<BodyPartRecord> GetValidBodyPartsFor(Pawn pawn)
        {
            IEnumerable<BodyPartRecord> availableOn = BillRecipeDef.Worker.GetPartsToApplyOn(pawn, BillRecipeDef);
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(BillRecipeDef, pawn).Concat(availableOn).Distinct();
        }

        public IEnumerable<BodyPartRecord> GetTheoreticallyValidBodyPartsFor()
            => BillUtils.GetFixedPartsToEverApplyOn(BillRecipeDef).Concat(ApplicablePartsGetter.GetFor(BillRecipeDef));
    }
}
