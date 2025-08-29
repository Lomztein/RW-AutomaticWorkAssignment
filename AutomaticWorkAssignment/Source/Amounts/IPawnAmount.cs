using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public interface IPawnAmount : IExposable
    {
        public string LabelCap { get; }
        public string Description { get; }
        public string Icon { get; }

        int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request);
    }
}
