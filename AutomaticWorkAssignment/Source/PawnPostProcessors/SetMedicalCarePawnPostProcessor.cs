using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetMedicalCarePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public MedicalCareCategory MedicalCare;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.playerSettings != null)
            {
                pawn.playerSettings.medCare = MedicalCare;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MedicalCare, "medicalCare");
        }
    }
}
