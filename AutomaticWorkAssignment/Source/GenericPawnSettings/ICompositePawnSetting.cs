using System.Collections.Generic;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public interface ICompositePawnSetting : IPawnSetting
    {
        IEnumerable<IPawnSetting> GetSettings();
    }
}
