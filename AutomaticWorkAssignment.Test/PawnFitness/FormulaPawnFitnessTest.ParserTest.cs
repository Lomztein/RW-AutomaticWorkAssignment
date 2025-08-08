using Lomzie.AutomaticWorkAssignment.PawnFitness;
using IToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.IToken;
using NameToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NameToken;
using NumberToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NumberToken;
using Operator = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;
using OperatorToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.OperatorToken;

namespace Lomzie.AutomaticWorkAssignment.Test.PawnFitness
{
    public class FormulaPawnFitnessTest
    {
        public class ParserTest
        {
            private static IEnumerable<object> NormalizeTestObjects(IEnumerable<object> objects) => objects.Select<object, object>(token => token switch
            {
                string name => name,
                double num => num,
                int num => (double)num,
                Operator op => op,
                _ => throw new InvalidCastException()
            });
            private static IEnumerable<IToken> TokensFromTestObjects(IEnumerable<object> objects) => NormalizeTestObjects(objects).Select<object, IToken>(token => token switch
            {
                string name => new NameToken(name),
                double num => new NumberToken(num),
                Operator op => new OperatorToken(op),
                _ => throw new InvalidCastException()
            });
            private static IEnumerable<object> TestObjectFromTokens(IEnumerable<IToken> tokens) => tokens.Select<IToken, object>(token => token switch
            {
                NameToken t => t.Value,
                NumberToken t => t.Value,
                OperatorToken t => t.Value,
                _ => throw new InvalidCastException()
            });

            [
                Theory,
                InlineData("1", new object[] { 1 }),
                InlineData("2.3", new object[] { 2.3 }),
                InlineData("2 + 3.3", new object[] { 2, Operator.Sum, 3.3 }),
                InlineData("8 / 6", new object[] { 8, Operator.Divide, 6 }),
                InlineData("83 % (6-4)", new object[] { 83, Operator.Modulus, Operator.OpenGroup, 6, Operator.Subtract, 4, Operator.CloseGroup }),
                InlineData("AVG(6, 3)", new object[] { "AVG", Operator.OpenGroup, 6, Operator.ArgSep, 3, Operator.CloseGroup }),
                InlineData("6** 3", new object[] { 6, Operator.Exp, 3 }),
                InlineData("SQRT(2)", new object[] { "SQRT", Operator.OpenGroup, 2, Operator.CloseGroup }),
                InlineData("ROOT(2, 2)", new object[] { "ROOT", Operator.OpenGroup, 2, Operator.ArgSep, 2, Operator.CloseGroup }),
            ]
            public void ShouldTokenizeValidFormula(string formula, object[] expected)
            {
                var returned = FormulaPawnFitness.Parser.TokenizeFormula(formula);
                Assert.Equal(NormalizeTestObjects(expected), TestObjectFromTokens(returned));
            }

            [
                Theory,
                InlineData("@", typeof(ArgumentException)),
                InlineData("1a", typeof(ArgumentException)),
            ]
            public void ShouldThrowOnTokenizeInvalidFormula(string formula, Type exceptionType)
            {
                var ex = Assert.Throws(exceptionType, () => FormulaPawnFitness.Parser.TokenizeFormula(formula).ToList());
            }
        }
    }
}