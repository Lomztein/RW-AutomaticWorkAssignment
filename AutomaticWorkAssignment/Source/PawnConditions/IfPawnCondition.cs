using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class IfPawnCondition : PawnSetting, IPawnCondition
    {
        public IPawnCondition IfCondition;
        public IPawnCondition TrueCondition;
        public IPawnCondition FalseCondition;
        public IPawnCondition ElseCondition;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (IfCondition == null)
                return false;

            if (IfCondition.IsValid(pawn, specification, request))
            {
                return TrueCondition?.IsValid(pawn, specification, request) ?? false;
            }
            else if (ElseCondition?.IsValid(pawn, specification, request) ?? true)
            {
                return FalseCondition?.IsValid(pawn, specification, request) ?? false;
            }

            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref IfCondition, "ifCondition");
            Scribe_Deep.Look(ref TrueCondition, "trueFitness");
            Scribe_Deep.Look(ref FalseCondition, "falseFitness");
            Scribe_Deep.Look(ref ElseCondition, "elseCondition");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!IfCondition.IsValidAfterLoad())
                    IfCondition = null;
                if (!TrueCondition.IsValidAfterLoad())
                    TrueCondition = null;
                if (!FalseCondition.IsValidAfterLoad())
                    FalseCondition = null;
                if (!ElseCondition.IsValidAfterLoad())
                    ElseCondition = null;
            }
        }
    }
}
