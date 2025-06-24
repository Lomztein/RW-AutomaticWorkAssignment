using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Logger
    {
        public static bool Enabled = true; // TODO: Make this a setting.
                                           // TODO: Extend to include log levels.

        public static void Write (string message)
        {
            Log.Message(message);
        }
    }
}
