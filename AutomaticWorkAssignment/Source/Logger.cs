using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Logger
    {
        // TODO: Extend to include log levels.
        public static bool Enabled => Settings.LogEnabled;

        public static void Message(string message)
        {
            if (Enabled)
            {
                Log.Message(message);
            }
        }
    }
}
