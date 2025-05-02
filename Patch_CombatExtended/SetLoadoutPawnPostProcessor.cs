using CombatExtended;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CombatExtended
{
    public class SetLoadoutPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Loadout Loadout;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Loadout != null)
            {
                Utility_Loadouts.SetLoadout(pawn, Loadout);
            }
        }
    }
}