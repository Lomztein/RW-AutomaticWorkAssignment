using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PlanetLayerPawnCondition : PawnSetting, IPawnCondition
    {
        public PlanetLayerDef LayerDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Map != null && pawn.Map.Tile != null)
            {
                return pawn.Map.Tile.LayerDef == LayerDef;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref LayerDef, "layerDef");
        }
    }
}
