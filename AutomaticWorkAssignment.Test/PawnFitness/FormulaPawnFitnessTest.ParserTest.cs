using Lomzie.AutomaticWorkAssignment.PawnFitness;
using IToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.IToken;
using NameToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NameToken;
using NumberToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NumberToken;
using Operator = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;
using OperatorToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.OperatorToken;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;

namespace Lomzie.AutomaticWorkAssignment.Test.PawnFitness
{
    public class FormulaPawnFitnessTest
    {
        public class ParserTest
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

            private static IEnumerable<object> NormalizeTestObjects(IEnumerable<object> objects) =>
                objects.Select<object, object>(token =>
                    token switch
                    {
                        string name => name,
                        double num => num,
                        int num => (double)num,
                        Operator op => op,
                        _ => throw new InvalidCastException(),
                    }
                );

            private static IEnumerable<IToken> TokensFromTestObjects(IEnumerable<object> objects) =>
                NormalizeTestObjects(objects)
                    .Select<object, IToken>(token =>
                        token switch
                        {
                            string name => new NameToken(name),
                            double num => new NumberToken(num),
                            Operator op => new OperatorToken(op),
                            _ => throw new InvalidCastException(),
                        }
                    );

            private static IEnumerable<object> TestObjectFromTokens(IEnumerable<IToken> tokens) =>
                tokens.Select<IToken, object>(token =>
                    token switch
                    {
                        NameToken t => t.Value,
                        NumberToken t => t.Value,
                        OperatorToken t => t.Value,
                        _ => throw new InvalidCastException(),
                    }
                );

            [
                Theory,
                InlineData("1", new object[] { 1 }),
                InlineData("2.3", new object[] { 2.3 }),
                InlineData("2 + 3.3", new object[] { 2, Sum, 3.3 }),
                InlineData("8 / 6", new object[] { 8, Divide, 6 }),
                InlineData(
                    "83 % (6-4)",
                    new object[]
                    {
                        83,
                        Modulus,
                        OpenGroup,
                        6,
                        Subtract,
                        4,
                        CloseGroup,
                    }
                ),
                InlineData(
                    "AVG(6, 3)",
                    new object[]
                    {
                        "AVG",
                        OpenGroup,
                        6,
                        ArgSep,
                        3,
                        CloseGroup,
                    }
                ),
                InlineData("6** 3", new object[] { 6, Exp, 3 }),
                InlineData(
                    "SQRT(2)",
                    new object[] { "SQRT", OpenGroup, 2, CloseGroup }
                ),
                InlineData(
                    "ROOT(2, 2)",
                    new object[]
                    {
                        "ROOT",
                        OpenGroup,
                        2,
                        ArgSep,
                        2,
                        CloseGroup,
                    }
                ),
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
                var ex = Assert.Throws(
                    exceptionType,
                    () => FormulaPawnFitness.Parser.TokenizeFormula(formula).ToList()
                );
            }

            [
                Theory,
                InlineData(new object[] { 1 }, 1),
                InlineData(new object[] { 1, Sum, 1 }, 2),
                #region Unary negative
                InlineData(new object[] { Subtract, 1 }, -1),
                InlineData(new object[] { OpenGroup, Subtract, 1, CloseGroup }, -1),
                #endregion Unary negative

                #region Priorities
                InlineData(new object[] { 2, Sum, 3, Factor, 4 }, 14),                                                                                                                             // Multiplication > Addition  
                InlineData(new object[] { 3, Factor, 2, Sum, 4 }, 10),                                                                                                                             // Same as above  
                InlineData(new object[] { 2, Exp, 3 }, 8, Skip = "TODO"),                                                                                                                                                  // Exponentiation  
                InlineData(new object[] { OpenGroup, 2, Sum, 3, CloseGroup, Factor, 4 }, 20, Skip = "TODO"),                                                                                    // Parentheses override precedence  
                InlineData(new object[] { 83, Modulus, OpenGroup, 6, Subtract, 4, CloseGroup }, 1, Skip = "TODO"),                                                                              // Parentheses and modulo  
                InlineData(new object[] { 10, Subtract, 5, Factor, 2 }, 0, Skip = "TODO"),                                                                                                                        // Multiplication before subtraction  
                InlineData(new object[] { OpenGroup, 5, Sum, 3, CloseGroup, Factor, 2 }, 16, Skip = "TODO"),                                                                                    // Parentheses again  
                InlineData(new object[] { 2, Sum, 3, Exp, 2 }, 11, Skip = "TODO"),                                                                                                                                // Exponentiation > Addition  
                InlineData(new object[] { 2, Factor, 3, Sum, 4, Divide, 2 }, 10, Skip = "TODO"),                                                                                                         // Multiplication and division > addition  
                InlineData(new object[] { OpenGroup, 2, Sum, 3, CloseGroup, Root, 5 }, 1, Skip = "TODO"),                                                                                       // Integer division  
                InlineData(new object[] { 2, Exp, 3, Exp, 2 }, 512, Skip = "TODO"),                                                                                                                               // Right-to-left exponentiation  
                InlineData(new object[] { 4, Root, 2 }, 2, Skip = "TODO"),                                                                                                                                                 // Integer division  
                InlineData(new object[] { 2, Exp, 3, Sum, 4, Root, 2 }, 10, Skip = "TODO"),                                                                                                              // Exponentiation and division > addition  
                InlineData(new object[] { OpenGroup, 2, Sum, 3, CloseGroup, Exp, OpenGroup, 4, Subtract, 2, CloseGroup }, 25, Skip = "TODO"),                        // Parentheses and exponentiation  
                InlineData(new object[] { OpenGroup, 8, Root, 2, CloseGroup, Sum, OpenGroup, 3, Factor, 2, CloseGroup }, 8, Skip = "TODO"),                          // Mixed operations  
                InlineData(new object[] { 100, Root, 3 }, 33, Skip = "TODO"),                                                                                                                                              // Integer division with remainder  
                InlineData(new object[] { 2, Exp, OpenGroup, 3, Sum, 4, Root, 2, CloseGroup, }, 10, Skip = "TODO"),                                                                    // Exponentiation and division > addition  
                InlineData(new object[] { 2, Exp, OpenGroup, 3, Sum, 1, CloseGroup }, 16, Skip = "TODO"),                                                                                       // Exponentiation with parentheses  
                InlineData(new object[] { OpenGroup, 2, Sum, 3, CloseGroup, Factor, OpenGroup, 4, Subtract, 2, CloseGroup, Factor, 2 }, 20, Skip = "TODO"), // Parentheses with underscores (interpreted as parentheses)  
                InlineData(new object[]{"(2 + 3) ** 2 // (3 + 2)"}, 5, Skip = "TODO"),                                                                                                                                              // Division with parentheses
                #endregion Priorities

                InlineData(new object[] { OpenGroup, 1, CloseGroup }, 1),
            ]
            public void ShouldParseTokens(object[] tokens, float expected)
            {
                var context = new FormulaPawnFitness.Parser.Context();
                var result = context.ParseTokens(TokensFromTestObjects(tokens)).ToFormula();
                Assert.Equal(expected, result.Calc(null, null, null));
            }

            [
                Theory,
                InlineData(new object[] { }, typeof(InvalidOperationException), "No tokens"),
                InlineData(new object[] { "asd" }, typeof(ArgumentException)),
                InlineData(new object[] { OpenGroup }, typeof(ArgumentException)),
            ]
            public void ShouldFailOnInvalidTokens(
                object[] tokens,
                Type expectedError,
                string? message = null
            )
            {
                var context = new FormulaPawnFitness.Parser.Context();
                var exception = Assert.Throws(
                    expectedError,
                    () => context.ParseTokens(TokensFromTestObjects(tokens)).ToFormula()
                );
                if (message != null)
                {
                    Assert.Equal(message, exception.Message);
                }
            }

            [
                Theory(),
                #region Priorities
                InlineData("2 + 3 * 4", 14),                 // Multiplication > Addition
                InlineData("3 * 2 + 4", 10),                 // Same as above
                InlineData("2 ** 3", 8),                     // Exponentiation
                InlineData("(2 + 3) * 4", 20),               // Parentheses override precedence
                InlineData("83 % (6 - 4)", 1),               // Parentheses and modulo
                InlineData("10 - 5 * 2", 0),                 // Multiplication before subtraction
                InlineData("(5 + 3) * 2", 16),               // Parentheses again
                InlineData("2 + 3 ** 2", 11),                // Exponentiation > Addition
                InlineData("2 * 3 + 4 / 2", 8),              // Multiplication and division > addition
                InlineData("(2 + 3) // 5", 1),               // Integer division
                InlineData("2 ** 3 ** 2", 512),              // Right-to-left exponentiation
                InlineData("4 // 2", 2),                     // Integer division
                InlineData("2 ** 3 + 4 // 2", 10),           // Exponentiation and division > addition
                InlineData("(2 + 3) ** (4 - 2)", 25),        // Parentheses and exponentiation
                InlineData("(8 // 2) + (3 * 2)", 8),         // Mixed operations
                InlineData("100 // 3", 33),                  // Integer division with remainder
                InlineData("2 ** (3 + 1)", 16),              // Exponentiation with parentheses
                InlineData("(2 + 3) * (4 - 2) ** 2", 20),    // Nested parentheses and exponentiation
                InlineData("(6 - 4) ** 2", 4),               // Exponentiation after subtraction
                InlineData("(5 + 2) // 3", 2),               // Integer division with parentheses
                InlineData("(2 + 3) ** (4 // 2)", 12),       // Exponentiation and division
                InlineData("16 // (4 - 2) ** 2", 3),         // Parentheses and exponentiation
                InlineData("2 ** 3 ** 4 // 2", 512),         // Right-to-left exponentiation and root
                InlineData("(2 ** 3) ** 2", 64),             // Exponentiation grouping
                InlineData("(2 * 3) + (4 / 2)", 8),          // Multiplication and division
                InlineData("(5 + 2) // 3 + 4", 6),           // Integer division and addition
                InlineData("(2 + 3) * 2 ** 3", 40),          // Parentheses and exponentiation
                InlineData("(2 + 3) * (4 // 2) ** 2", 50),   // Parentheses and nested operations
                InlineData("(2 + 3) ** (4 // 2) ** 2", 625), // Right-to-left exponentiation with parentheses
                InlineData("(6 + 2) ** 2 // 3", 4),          // Exponentiation and root
                InlineData("(10 - 2) ** (4 // 2)", 64),      // Integer division and exponentiation
                InlineData("(2 ** 3) ** 2 // 3", 170),       // Exponentiation and division
                InlineData("(2 + 3) ** 2 // (3 + 2)", 5),    // Division with parentheses
                #endregion
            ]
            public void ShouldReturnCorrectValue(string formula, float expected) {
                var formulaExpression = FormulaPawnFitness.Parser.ParseFormula(formula);
                var linqExpr = formulaExpression.Expression.ToString();
                var result = formulaExpression.Calc(null, null, null);
                Assert.Equal(expected, result);
            }
        }
    }
}
