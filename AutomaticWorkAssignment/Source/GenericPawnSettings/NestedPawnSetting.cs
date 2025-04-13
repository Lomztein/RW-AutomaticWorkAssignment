using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class NestedPawnSetting : IPawnSetting
    {
        public abstract string Label { get; }
        public abstract string Description { get; }

        public IPawnSetting InnerSetting;

        public void ExposeData()
        {
            Scribe_Deep.Look(ref InnerSetting, "innerCondition");
        }
    }
}
