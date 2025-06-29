using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class ResolveWorkRequest
    {
        public List<Pawn> Pawns;
        public Map Map;
        public WorkManager WorkManager;
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
