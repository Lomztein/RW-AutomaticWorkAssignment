using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class PercentagePawnAmount : IPawnAmount
    {
        public float Percentage;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Percentage, "percentage");
        }

        public int GetCount()
        {
            int colonistCount = WorkManager.Instance.GetAllAssignablePawns().Count();
            return (int)(colonistCount * Percentage);
        }
    }
}
