using CombatExtended;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.CombatExtended
{
    public class SetLoadoutPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Loadout Loadout;
        private string _loadoutName;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && Loadout != null)
            {
                Utility_Loadouts.SetLoadout(pawn, Loadout);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Loadout, "loadout");

            if (Scribe.mode == LoadSaveMode.Saving)
                _loadoutName = Loadout?.label;

            Scribe_Values.Look(ref _loadoutName, "loadoutName");

            if (Scribe.mode != LoadSaveMode.Saving && Loadout == null && !string.IsNullOrWhiteSpace(_loadoutName))
                Loadout = LoadoutManager.Loadouts.FirstOrDefault(x => x.label == _loadoutName);
        }
    }
}
