using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class RoyaltyTitlePawnCondition : PawnSetting, IPawnCondition
    {
        public Faction Faction;
        public RoyalTitleDef TitleDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.royalty != null && TitleDef != null)
            {
                if (Faction != null)
                {
                    return pawn.royalty.HasTitle(TitleDef);
                }
                else
                {
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
        }
    }
}
