using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class MeditationFocusPawnCondition : PawnSetting, IPawnCondition
    {
        public bool RequireAll;
        public List<MeditationFocusDef> FocusTypes = new List<MeditationFocusDef>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref RequireAll, "requireAll", true);
            Scribe_Collections.Look(ref FocusTypes, "focusTypes");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null) return false;
            if (RequireAll)
            {
                return FocusTypes.All(x => MeditationFocusTypeAvailabilityCache.PawnCanUse(pawn, x));
            }
            else
            {
                return FocusTypes.Any(x => MeditationFocusTypeAvailabilityCache.PawnCanUse(pawn, x));
            }
        }
    }
}
