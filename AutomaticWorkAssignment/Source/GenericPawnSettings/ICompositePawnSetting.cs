using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public interface ICompositePawnSetting : IPawnSetting
    {
        IEnumerable<IPawnSetting> GetSettings();
    }
}
