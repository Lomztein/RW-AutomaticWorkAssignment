using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class FavorPawnFitness : PawnSetting, IPawnFitness
    {
        public Faction Faction;
        private string _factionName;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.royalty != null)
            {
                if (Faction == null)
                    return GetHighestFavor(pawn);
                return pawn.royalty.GetFavor(Faction);
            }
            return 0f;
        }

        private float GetHighestFavor(Pawn pawn)
        {
            var favor = Find.FactionManager.AllFactionsListForReading
                .Where(x => x != null)
                .Select(x => pawn.royalty.GetFavor(x));

            if (favor.Count() == 0) return 0;
            return favor.Max();
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
