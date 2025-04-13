using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class IntPawnAmount : IPawnAmount
    {
        public int Value;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Value, "value");
        }

        public int GetCount()
        {
            return Value;
        }
    }
}
