using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public interface IPawnAmount : IExposable
    {
        int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request);
    }
}
