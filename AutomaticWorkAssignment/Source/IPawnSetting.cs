using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public interface IPawnSetting : IExposable
    {
        string Label { get; }
        string Description { get; }
    }
}
