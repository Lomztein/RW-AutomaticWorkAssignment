using AutomaticWorkAssignment;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.GenericPawnSettings
{
    public abstract class NestedPawnSetting : PawnSetting, IPawnSetting
    {
        public IPawnSetting InnerSetting;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref InnerSetting, "innerCondition");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (InnerSetting != null && !InnerSetting.IsValidAfterLoad())
                    InnerSetting = null;
            }
        }
    }
}
