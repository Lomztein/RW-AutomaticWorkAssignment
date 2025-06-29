using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class FavorPawnFitness : PawnSetting, IPawnFitness
    {
        public Faction Faction;

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
        }
    }
}
