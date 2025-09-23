using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class RoyaltyTitlePawnCondition : PawnSetting, IPawnCondition
    {
        public Faction Faction;
        public RoyalTitleDef TitleDef;

        private string _factionName;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.royalty != null)
            {
                if (Faction == null)
                {
                    if (TitleDef == null)
                        return pawn.royalty.AllTitlesForReading.Any();
                    return pawn.royalty.HasTitle(TitleDef);
                }
                else
                {
                    if (TitleDef == null)
                        pawn.royalty.HasAnyTitleIn(Faction);
                    return pawn.royalty.AllTitlesInEffectForReading.Any(x => x.def == TitleDef && x.faction == Faction);
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Faction, "faction");
            Scribe_Defs.Look(ref TitleDef, "titleDef");

            if (Scribe.mode == LoadSaveMode.Saving)
                _factionName = Faction?.Name;
            
            Scribe_Values.Look(ref _factionName, "factionName");

            if (Scribe.mode != LoadSaveMode.Saving && Faction == null && _factionName != null)
                Faction = Current.Game.World.factionManager.AllFactions.FirstOrDefault(x => x.Name == _factionName);
        }
    }
}
