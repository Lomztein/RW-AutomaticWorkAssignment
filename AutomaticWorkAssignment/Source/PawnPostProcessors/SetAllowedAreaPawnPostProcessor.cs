using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetAllowedAreaPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Area AllowedArea;
        private string _allowedAreaName;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref AllowedArea, "area");

            if (Scribe.mode == LoadSaveMode.Saving && AllowedArea != null)
                _allowedAreaName = AllowedArea.Label;

            Scribe_Values.Look(ref _allowedAreaName, "allowedAreaName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (AllowedArea == null && _allowedAreaName != null)
                {
                    AllowedArea = Find.CurrentMap.areaManager.AllAreas.Find(x => x.Label == _allowedAreaName);
                }
            }
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn.Map?.IsPlayerHome ?? false)
            {
                pawn.playerSettings.AreaRestrictionInPawnCurrentMap = AllowedArea;
            }
        }
    }
}
