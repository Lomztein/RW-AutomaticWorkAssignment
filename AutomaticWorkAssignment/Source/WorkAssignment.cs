using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class WorkAssignment
    {
        public WorkSpecification Specification;
        public Pawn Pawn;

        public int Index;
        public bool IsCritical;

        public WorkAssignment Substitution;
        public bool IsSubstituted => Substitution != null;

        private Dictionary<WorkTypeDef, int> _setPriorities = new Dictionary<WorkTypeDef, int>();

        public WorkAssignment(WorkSpecification specification, Pawn pawn, int index, bool isCritical)
        {
            Specification = specification;
            Pawn = pawn;
            Index = index;
            IsCritical = isCritical;
        }

        public int? GetPriority(WorkTypeDef workType)
        {
            if (IsSubstituted)
                return Substitution.GetPriority(workType);
            if (_setPriorities.TryGetValue(workType, out var priority))
                return priority;
            return null;
        }

        public void SetPriority(WorkTypeDef workType, int priority)
        {
            _setPriorities[workType] = priority;
        }

        public void ClearPriorities()
        {
            _setPriorities.Clear();
        }

        public (WorkTypeDef, int) GetHighestPriority ()
        {
            if (IsSubstituted)
                return Substitution.GetHighestPriority();
            if (_setPriorities.Count == 0)
                return (null, -1);
            var min = _setPriorities.MinBy(x => x.Value);
            return (min.Key, min.Value);
        }

        public (WorkTypeDef, int) GetLowestPriority()
        {
            if (IsSubstituted)
                return Substitution.GetLowestPriority();
            if (_setPriorities.Count == 0)
                return (null, -1);
            var max = _setPriorities.MaxBy(x => x.Value);
            return (max.Key, max.Value);
        }

        public void SubstituteWith(WorkAssignment substitution)
            => Substitution = substitution;
    }
}
