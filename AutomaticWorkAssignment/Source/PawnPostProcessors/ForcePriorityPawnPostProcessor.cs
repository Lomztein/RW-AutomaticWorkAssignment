using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class ForcePriorityPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public WorkTypeDef WorkType;
        public int Priority;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (WorkType != null)
            {
                pawn.workSettings.SetPriority(WorkType, Priority);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref WorkType, "workType");
            Scribe_Values.Look(ref Priority, "priority", 0);
        }
    }
}
