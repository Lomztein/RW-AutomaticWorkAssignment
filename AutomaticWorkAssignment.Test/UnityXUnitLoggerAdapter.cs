using UnityEngine;
using Xunit.Abstractions;

namespace Lomzie.AutomaticWorkAssignment.Test
{
    internal class UnityXUnitLoggerAdapter : ILogHandler
    {
        public UnityXUnitLoggerAdapter(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        public ITestOutputHelper TestOutputHelper { get; }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            throw new NotImplementedException();
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            TestOutputHelper.WriteLine($"{logType}: {string.Format(format, args)}");
        }
    }
}
