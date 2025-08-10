using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    /// <summary>
    /// Defines a condition that must be met for a pawn to be assigned work.
    /// </summary>
    public interface IPawnCondition : IPawnSetting
    {
        bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request);
    }
}
