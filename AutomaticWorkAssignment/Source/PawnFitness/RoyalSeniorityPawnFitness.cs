using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class RoyalSeniorityPawnFitness : PawnSetting, IPawnFitness
    {
        public Faction Faction;
        private string _factionName;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.royalty != null)
            {
                if (Faction == null)
                {
                    if (pawn.royalty.AllAbilitiesForReading.Count > 0)
                    {
                        return pawn.royalty.AllTitlesInEffectForReading.Max(x => x.def.seniority);
                    }
                    else
                    {
                        return 0;
                    }
                }
                return pawn.royalty.GetCurrentTitle(Faction)?.seniority ?? 0;
            }
            return 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Faction, "faction");

            if (Scribe.mode == LoadSaveMode.Saving)
                _factionName = Faction?.Name;

            Scribe_Values.Look(ref _factionName, "factionName");

            if (Scribe.mode != LoadSaveMode.Saving && Faction == null && _factionName != null)
                Faction = Current.Game.World.factionManager.AllFactions.FirstOrDefault(x => x.Name == _factionName);
        }
    }
}
