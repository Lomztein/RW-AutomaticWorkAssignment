using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.Events;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class OnEventPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public EventDef EventDef;
        public IPawnPostProcessor NestedPostProcessor;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && EventDef != null && NestedPostProcessor != null)
            {
                EventManager.Subscribe(pawn, EventDef, workSpecification, NestedPostProcessor);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref EventDef, "eventDef");
            Scribe_Deep.Look(ref NestedPostProcessor, "nestedPostProcessor");
        }
    }
}
