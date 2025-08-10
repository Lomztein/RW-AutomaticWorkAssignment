using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using System;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public abstract class PawnSetting : IPawnSetting
    {
        public string Label => Def.LabelCap;
        public string Description => Def.description;

        public PawnSettingDef Def;

        public static T CreateFrom<T>(PawnSettingDef def) where T : IPawnSetting
        {
            T setting = (T)Activator.CreateInstance(def.settingClass);
            if (setting is PawnSetting pawnSetting)
            {
                pawnSetting.Def = def;
            }
            return setting;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "def");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (Def == null)
                {
                    Def = DefDatabase<PawnSettingDef>.AllDefs.First(x => x.settingClass == GetType());
                }
            }
        }
    }
}
