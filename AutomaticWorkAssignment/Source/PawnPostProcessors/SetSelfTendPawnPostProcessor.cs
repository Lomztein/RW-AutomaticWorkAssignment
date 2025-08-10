using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetSelfTendPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public bool SelfTend;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.playerSettings != null)
            {
                pawn.playerSettings.selfTend = SelfTend;
            }
        }

        public static string GetLabel(bool value)
            => (value ? "AWA.True" : "AWA.False").Translate();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SelfTend, "selfTend");
        }
    }
}
