using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetPawnColonistBarOrderingPawnPostProcessor : NestedPawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            IPawnFitness fitness = InnerSetting as IPawnFitness;
            if (pawn != null && pawn.playerSettings != null && fitness != null)
            {
                pawn.playerSettings.displayOrder = (int)fitness.CalcFitness(pawn, workSpecification, request);
                Find.ColonistBar.MarkColonistsDirty();
            }
        }
    }
}
