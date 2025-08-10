using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public interface IPawnSetting : IExposable
    {
        string Label { get; }
        string Description { get; }
    }
}
