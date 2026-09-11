using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events.Watchers
{
    public abstract class StateWatcher<TWatcherComponent> : Watcher<TWatcherComponent>
    {
        private bool _value;
        private bool _initialized;

        public override void Tick(TWatcherComponent component)
        {
            bool newValue = GetCurrentState(component);
            if (newValue != _value && _initialized)
            {
                _value = newValue;
                OnEventChanged(newValue, component);
            }
            if (!_initialized)
                _initialized = true;
        }

        public abstract bool GetCurrentState(TWatcherComponent watcherComponent);

        public abstract void OnEventChanged(bool newValue, TWatcherComponent watcherComponent);
    }
}
