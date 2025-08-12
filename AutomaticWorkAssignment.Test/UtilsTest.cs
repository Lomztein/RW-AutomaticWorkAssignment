using UnityEngine;
using Verse;
using Xunit.Abstractions;

namespace Lomzie.AutomaticWorkAssignment.Test
{
    public class UtilsTest
    {
        public UtilsTest(ITestOutputHelper testOutputHelper)
        {
            Debug.unityLogger.logHandler = new UnityXUnitLoggerAdapter(testOutputHelper);
        }

        [Fact, Trait("Function", nameof(Utils.SplitIn))]
        public void SplitIn2_2()
        {
            var rect = new RectDivider(new(10, 10, 10, 10), 1, Vector2.zero);
            var split = rect.SplitIn(x: 2, y: 2);
            Assert.Equal(4, split.Length);
            Assert.Equal(
                [new Rect(10, 10, 5, 5), new Rect(15, 10, 5, 5), new Rect(10, 15, 5, 5), new Rect(15, 15, 5, 5)],
                split.Select(rect => rect.Rect).ToArray());
        }

    }
}
