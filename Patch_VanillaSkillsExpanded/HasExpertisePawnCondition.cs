using AutomaticWorkAssignment.UI.Generic;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI;
using Verse;
using VSE;
using VSE.Expertise;

namespace Lomzie.AutomaticWorkAssignment.Patches.VanillaSkillsExpanded
{
    [StaticConstructorOnStartup]
    public class HasExpertisePawnCondition : PawnSetting, IPawnCondition
    {
        public ExpertiseDef ExpertiseDef;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                ExpertiseTracker tracker = ExpertiseTrackers.Expertise(pawn);
                if (tracker != null)
                {
                    ExpertiseRecord record = tracker.AllExpertise.Find(x => x.def == ExpertiseDef);
                    return record != null;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref ExpertiseDef, "expertiseDef");
        }

        static HasExpertisePawnCondition()
        {
            PawnSettingUIHandlers.AddHandler(new PickerPawnSettingUIHandler<HasExpertisePawnCondition, ExpertiseDef>(
                () => DefDatabase<ExpertiseDef>.AllDefs,
                (x) => x?.label,
                (x) => x.ExpertiseDef?.label ?? "Select expertise",
                (x, def) => x.ExpertiseDef = def
                ));
        }
    }
}