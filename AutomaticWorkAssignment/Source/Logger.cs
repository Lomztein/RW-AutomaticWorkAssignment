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
        // TODO: Extend to include log levels.
        public static bool Enabled => AutomaticWorkAssignmentSettings.LogEnabled;

        public static void Message (string message)
        {
            if (Enabled)
            {
                Log.Message(message);
            }
        }
    }
}
