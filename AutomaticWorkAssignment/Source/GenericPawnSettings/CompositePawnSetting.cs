using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class CompositePawnSetting<TSettingType> : PawnSetting, ICompositePawnSetting where TSettingType : IPawnSetting
    {
        public virtual int MaxSettings => int.MaxValue;

        public List<TSettingType> InnerSettings = new List<TSettingType>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref InnerSettings, "innerSettings");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (InnerSettings == null)
                    InnerSettings = new List<TSettingType>();
                InnerSettings = InnerSettings.Where(x => x.IsValidAfterLoad()).ToList();
            }
        }

        public IEnumerable<IPawnSetting> GetSettings()
            => InnerSettings.Cast<IPawnSetting>();
    }
}
