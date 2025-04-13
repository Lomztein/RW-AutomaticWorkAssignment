using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class CompositePawnSetting : IPawnSetting
    {
        public abstract string Label { get; }
        public abstract string Description { get; }
        public virtual int MaxSettings => int.MaxValue;

        public List<IPawnSetting> InnerSettings = new List<IPawnSetting>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref InnerSettings, "innerSettings");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (InnerSettings == null)
                    InnerSettings = new List<IPawnSetting>();
            }
        }
    }
}
