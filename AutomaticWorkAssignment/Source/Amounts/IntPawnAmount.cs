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

        public int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            return Value;
        }
    }
}
