using Lomzie.AutomaticWorkAssignment.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public abstract class PawnAmount : IPawnAmount
    {
        public PawnAmountDef Def;

        public string LabelCap => Def.LabelCap;
        public string Description => Def.description;
        public string Icon => Def.icon;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "def");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Def ??= DefDatabase<PawnAmountDef>.AllDefs.FirstOrDefault(x => x.defClass == GetType());
            }
        }

        public abstract int GetCount(WorkSpecification workSpecification, ResolveWorkRequest request);

        public static IPawnAmount CreateFrom(PawnAmountDef def)
        {
            IPawnAmount instance = (IPawnAmount)Activator.CreateInstance(def.defClass);
            if (instance is PawnAmount pawnAmount)
                pawnAmount.Def = def;
            return instance;
        }
    }
}
