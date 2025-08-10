﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class CompositePawnSetting : PawnSetting, ICompositePawnSetting
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
                InnerSettings = InnerSettings.Where(x => x.IsValidAfterLoad()).ToList();
            }
        }

        public IEnumerable<IPawnSetting> GetSettings()
            => InnerSettings;
    }
}
