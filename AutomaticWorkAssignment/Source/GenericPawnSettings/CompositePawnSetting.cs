using AutomaticWorkAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class CompositePawnSetting : PawnSetting, IPawnSetting
    {
        public virtual int MaxSettings => int.MaxValue;

        public List<IPawnSetting> InnerSettings = new List<IPawnSetting>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref InnerSettings, "innerSettings");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (InnerSettings == null)
                    InnerSettings = new List<IPawnSetting>();
            }
        }
    }
}
