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
