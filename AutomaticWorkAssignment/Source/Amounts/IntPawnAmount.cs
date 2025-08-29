using Verse;

namespace Lomzie.AutomaticWorkAssignment.Amounts
{
    public class IntPawnAmount : PawnAmount
    {
        public int Value;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Value, "value");
        }

        public override int GetCount(WorkSpecification spec, ResolveWorkRequest req)
        {
            return Value;
        }
    }
}
