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
        public void SplitIn2_2_NoMargin()
        {
            var rect = new RectDivider(new(10, 10, 10, 10), 1, Vector2.zero);
            var split = rect.SplitIn(x: 2, y: 2);
            Assert.Equal(4, split.Length);
            Assert.Equal(
                [new Rect(10, 10, 5, 5), new Rect(15, 10, 5, 5), new Rect(10, 15, 5, 5), new Rect(15, 15, 5, 5)],
                split.Select(rect => rect.Rect).ToArray());
        }

        [Fact, Trait("Function", nameof(Utils.SplitIn))]
        public void SplitIn2_2_Margin()
        {
            var rect = new RectDivider(new(10, 10, 11, 11), 1, new(1, 1));
            var split = rect.SplitIn(x: 2, y: 2);
            Assert.Equal(4, split.Length);
            Assert.Equal(
                [new Rect(10, 10, 5, 5), new Rect(16, 10, 5, 5), new Rect(10, 16, 5, 5), new Rect(16, 16, 5, 5)], split.Select(rect => rect.Rect).ToArray());
        }
    }
}
