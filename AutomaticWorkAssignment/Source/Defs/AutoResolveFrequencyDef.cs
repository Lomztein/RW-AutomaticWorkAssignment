using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    public class AutoResolveFrequencyDef : Def
    {
        public enum ResolveCalender { None, Year, Quadrum, Day, Hour }

        public ResolveCalender resolveCalender;
        public int timeBetweenResolve;
    }
}
