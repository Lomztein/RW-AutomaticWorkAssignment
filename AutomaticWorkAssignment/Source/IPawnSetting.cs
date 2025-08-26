using Lomzie.AutomaticWorkAssignment.Defs;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public interface IPawnSetting : IExposable
    {
        PawnSettingDef Def { get; }
        string Label { get; }
        string Description { get; }
    }
}
