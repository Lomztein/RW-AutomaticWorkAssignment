using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public interface IPawnPostProcessor : IPawnSetting
    {
        void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request);
    }
}
