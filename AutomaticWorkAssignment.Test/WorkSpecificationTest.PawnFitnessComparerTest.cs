using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Moq;
using Verse;
using static Lomzie.AutomaticWorkAssignment.WorkSpecification;

namespace Lomzie.AutomaticWorkAssignment.Test
{
    public class WorkSpecificationTest
    {
        public class PawnFitnessComparerTest
        {
            [Fact]
            public void ShouldWorkForSimpleCase()
            {
                var comparer = new PawnFitnessComparer([], null, null);
                Assert.Equal(1, comparer.Compare(null, null));
            }

            [Theory, InlineData(false), InlineData(true)]
            public void ShouldBreakAsSoonAsDiff(bool inverse)
            {
                var pawn1 = new Pawn();
                var pawn2 = new Pawn();
                // Use the same fn 3 times
                var mockFn = new Mock<IPawnFitness>();
                mockFn
                    .Setup(_ => _.CalcFitness(pawn1, It.IsAny<WorkSpecification>(), It.IsAny<ResolveWorkRequest>()))
                    .Returns(new Queue<float>([42, 42, 666]).Dequeue);
                mockFn
                    .Setup(_ => _.CalcFitness(pawn2, It.IsAny<WorkSpecification>(), It.IsAny<ResolveWorkRequest>()))
                    .Returns(new Queue<float>([42, 420, 123]).Dequeue);
                var comparer = new PawnFitnessComparer([mockFn.Object, mockFn.Object, mockFn.Object], null, null);
                Assert.Equal(
                    inverse ? 1 : -1,
                    inverse ? comparer.Compare(pawn1, pawn2) : comparer.Compare(pawn2, pawn1));
                mockFn.Verify(
                    _ => _.CalcFitness(pawn1, It.IsAny<WorkSpecification>(), It.IsAny<ResolveWorkRequest>()),
                    Times.Exactly(2));
                mockFn.Verify(
                    _ => _.CalcFitness(pawn2, It.IsAny<WorkSpecification>(), It.IsAny<ResolveWorkRequest>()),
                    Times.Exactly(2));
            }
        }
    }
}
