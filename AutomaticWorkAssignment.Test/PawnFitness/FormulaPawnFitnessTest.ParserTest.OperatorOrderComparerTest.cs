using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Operator = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;

namespace Lomzie.AutomaticWorkAssignment.Test.PawnFitness
{
    public partial class FormulaPawnFitnessTest
    {
        public partial class ParserTest
        {
            public class OperatorOrderComparerTest
            {
                [
                    Theory,
                    InlineData(
                        new Operator[] { Sum, Subtract },
                        new int[] {      0,   1 }
                    ),
                    InlineData(
                        new Operator[] { Subtract, Sum },
                        new int[] {      0,        1 }
                    ),
                    InlineData(
                        new Operator[] { Sum, Divide },
                        new int[] {      1,   0 }
                    ),
                    InlineData(
                        new Operator[] { Divide, Sum },
                        new int[] {      0,      1 }
                    ),
                    InlineData(
                        new Operator[] { Exp, Factor },
                        new int[] {      0,   1 }
                    ),
                    InlineData(
                        new Operator[] { Factor, Exp },
                        new int[] {      1,      0 }
                    ),
                    InlineData(
                        new Operator[] { Exp, Exp },
                        new int[] {      1,      0 }
                    ),
                ]
                internal void ShouldTokenizeValidFormula(Operator[] source, int[] expected)
                {
                    var comparer = new FormulaPawnFitness.Parser.OperatorOrderComparer();
                    var actual = source
                        .Select((op, index) => (op, index))
                        .OrderBy(op => op.op, comparer)
                        .Select(op => op.index);
                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
