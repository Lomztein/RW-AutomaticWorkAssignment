using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class IfPawnFitness : PawnSetting, IPawnFitness
    {
        public IPawnCondition IfCondition;
        public IPawnFitness TrueFitness;
        public IPawnFitness FalseFitness;
        public IPawnCondition ElseCondition;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (IfCondition == null)
                return 0;

            if (IfCondition.IsValid(pawn, specification, request))
            {
                return TrueFitness?.CalcFitness(pawn, specification, request) ?? 0;
            }
            else if (ElseCondition?.IsValid(pawn, specification, request) ?? true)
            {
                return FalseFitness?.CalcFitness(pawn, specification, request) ?? 0;
            }

            return 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref IfCondition, "ifCondition");
            Scribe_Deep.Look(ref TrueFitness, "trueFitness");
            Scribe_Deep.Look(ref FalseFitness, "falseFitness");
            Scribe_Deep.Look(ref ElseCondition, "elseCondition");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!IfCondition.IsValidAfterLoad())
                    IfCondition = null;
                if (!TrueFitness.IsValidAfterLoad())
                    TrueFitness = null;
                if (!FalseFitness.IsValidAfterLoad())
                    FalseFitness = null;
                if (!ElseCondition.IsValidAfterLoad())
                    ElseCondition = null;
            }
        }
    }
}
