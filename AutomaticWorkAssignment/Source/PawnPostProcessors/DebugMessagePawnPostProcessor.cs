using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DebugMessagePawnPostProcessor : NestedPawnSetting, IPawnPostProcessor
    {
        public string Title = string.Empty;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            string message = InnerSetting.Label + ": ";
            if (InnerSetting != null)
            {
                if (InnerSetting is IPawnFitness fitness)
                    message += fitness.CalcFitness(pawn, workSpecification, request);
                if (InnerSetting is IPawnCondition condition)
                    message += condition.IsValid(pawn, workSpecification, request);
                if (InnerSetting is IPawnPostProcessor)
                    message += "Executed";
            }
            else
            {
                message = "Ahoy!";
            }

            Messages.Message(message, MessageTypeDefOf.NeutralEvent);
        }
    }
}
