using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class ResolveWorkRequest
    {
        public List<Pawn> Pawns;
        public Map Map;
        public MapWorkManager WorkManager;
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        public void SetVariable(string key, object value)
            => _variables.SetOrAdd(key, value);

        public T GetVariable<T>(string key)
        {
            if (_variables.TryGetValue(key, out var val))
            {
                return (T)val;
            }
            return default;
        }
    }
}
