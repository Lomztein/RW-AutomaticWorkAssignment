using Lomzie.AutomaticWorkAssignment.PawnFitness;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;
using IToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.IToken;
using NameToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NameToken;
using NumberToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.NumberToken;
using Operator = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.Operator;
using OperatorToken = Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser.OperatorToken;

namespace Lomzie.AutomaticWorkAssignment.Test.PawnFitness
{
    public partial class FormulaPawnFitnessTest
    {
        public partial class ParserTest
        {
            class FormulaTestCase(
                string description,
                string formula,
                string linqExpr,
                object[] syntheticTokens,
                double evaluationResult,
                Formula.Bindings? bindings = null
            )
            {
                public string Description { get; } = description;
                public string Formula { get; } = formula;
                public string LinqExpr { get; } = linqExpr;
                public object[] SyntheticTokens { get; } = syntheticTokens;
                public double EvaluationResult { get; } = evaluationResult;
                public Formula.Bindings? Bindings { get; } = bindings;
            }
            private static Formula.Bindings GetTestBindings(string label) =>
                setsMap.Values.SelectMany(set => set)
                    .Single(test => test.Description == label)
                    .Bindings ?? [];
            private static readonly FormulaTestCase[] basicTestCases;
            private static readonly FormulaTestCase[] bindingsTestCases;
            private static readonly Dictionary<string, FormulaTestCase[]> setsMap;

            static ParserTest()
            {
                basicTestCases =
                [
                    #region Literals
                    new FormulaTestCase("Literal",
                        formula: "1",
                        linqExpr: "1",
                        syntheticTokens: [1],
                        evaluationResult: 1
                    ),
                    new FormulaTestCase("Parenthized literal",
                        formula: "(1)",
                        linqExpr: "1",
                        syntheticTokens: [OpenGroup, 1, CloseGroup],
                        evaluationResult: 1
                    ),
                    new FormulaTestCase("Double literal",
                        formula: "2.3",
                        linqExpr: "2.3",
                        syntheticTokens: [2.3],
                        evaluationResult: 2.3
                    ),
                    new FormulaTestCase("Double literal addition",
                        formula: "2 + 3.3",
                        linqExpr: "(2 + 3.3)",
                        syntheticTokens: [2, Sum, 3.3],
                        evaluationResult: 5.3
                    ),
                    #endregion
                    new FormulaTestCase("Dumb addition",
                        formula: "1+ 1",
                        linqExpr: "(1 + 1)",
                        syntheticTokens: [1, Sum, 1],
                        evaluationResult: 2
                    ),
                    #region Unary negative
                    new FormulaTestCase("Negated literal",
                        formula: "-1",
                        linqExpr: "-1",
                        syntheticTokens: [Subtract, 1],
                        evaluationResult: -1
                    ),
                    new FormulaTestCase("Negated literal in parenthesis",
                        formula: "( -1)",
                        linqExpr: "-1",
                        syntheticTokens: [OpenGroup, Subtract, 1, CloseGroup],
                        evaluationResult: -1
                    ),
                    new FormulaTestCase("Negated parenthesis",
                        formula: "-( -1 + 3)",
                        linqExpr: "-(-1 + 3)",
                        syntheticTokens: [Subtract, OpenGroup, Subtract, 1, Sum, 3, CloseGroup],
                        evaluationResult: -2
                    ),
                    #endregion Unary negative

                    #region Priorities
                    new FormulaTestCase("Multiplication > Addition",
                        formula: "2+ 3 * 4",
                        linqExpr: "(2 + (3 * 4))",
                        syntheticTokens: [2, Sum, 3, Factor, 4],
                        evaluationResult: 14
                    ),
                    new FormulaTestCase("Same as above",
                        formula: "3 * 2+4",
                        linqExpr: "((3 * 2) + 4)",
                        syntheticTokens: [3, Factor, 2, Sum, 4],
                        evaluationResult: 10
                    ),
                    new FormulaTestCase("Exponentiation",
                        formula: "2**3",
                        linqExpr: "(2 ** 3)",
                        syntheticTokens: [2, Exp, 3],
                        evaluationResult: 8
                    ),
                    new FormulaTestCase("Parentheses override precedence",
                        formula: "(2+ 3)*4",
                        linqExpr: "((2 + 3) * 4)",
                        syntheticTokens: [OpenGroup, 2, Sum, 3, CloseGroup, Factor, 4],
                        evaluationResult: 20
                    ),
                    new FormulaTestCase("Parentheses and modulo",
                        formula: "83 % (6-4)",
                        linqExpr: "(83 % (6 - 4))",
                        syntheticTokens: [83, Modulus, OpenGroup, 6, Subtract, 4, CloseGroup],
                        evaluationResult: 1
                    ),
                    new FormulaTestCase("Multiplication before subtraction",
                        formula: "10 - 5*2",
                        linqExpr: "(10 - (5 * 2))",
                        syntheticTokens: [10, Subtract, 5, Factor, 2],
                        evaluationResult: 0
                    ),
                    new FormulaTestCase("Parentheses again",
                        formula: "(5+ 3)  *2",
                        linqExpr: "((5 + 3) * 2)",
                        syntheticTokens: [OpenGroup, 5, Sum, 3, CloseGroup, Factor, 2],
                        evaluationResult: 16
                    ),
                    new FormulaTestCase("Exponentiation > Addition",
                        formula: "2+3**2",
                        linqExpr: "(2 + (3 ** 2))",
                        syntheticTokens: [2, Sum, 3, Exp, 2],
                        evaluationResult: 11
                    ),
                    new FormulaTestCase("Multiplication and division > addition",
                        formula: "2*3+4/2",
                        linqExpr: "((2 * 3) + (4 / 2))",
                        syntheticTokens: [2, Factor, 3, Sum, 4, Divide, 2],
                        evaluationResult: 8
                    ),
                    new FormulaTestCase("Right-to-left exponentiation",
                        formula: "2**3**2",
                        linqExpr: "(2 ** (3 ** 2))",
                        syntheticTokens: [2, Exp, 3, Exp, 2],
                        evaluationResult: 512
                    ),
                    new FormulaTestCase("Square root",
                        formula: "4//2",
                        linqExpr: "Sqrt(4)",
                        syntheticTokens: [4, Root, 2],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Cubic root",
                        formula: "8//3",
                        linqExpr: "(8 ** (1 / 3))",
                        syntheticTokens: [8, Root, 3],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Exponentiation and root > addition",
                        formula: "2**3 + 4//2",
                        linqExpr: "((2 ** 3) + Sqrt(4))",
                        syntheticTokens: [2, Exp, 3, Sum, 4, Root, 2],
                        evaluationResult: 10
                    ),
                    new FormulaTestCase("Parentheses and exponentiation",
                        formula: "(2+3) ** (4-2)",
                        linqExpr: "((2 + 3) ** (4 - 2))",
                        syntheticTokens: [OpenGroup, 2, Sum, 3, CloseGroup, Exp, OpenGroup, 4, Subtract, 2, CloseGroup],
                        evaluationResult: 25
                    ),
                    new FormulaTestCase("Mixed operations",
                        formula: "(4//2) + (3*2)",
                        linqExpr: "(Sqrt(4) + (3 * 2))",
                        syntheticTokens: [OpenGroup, 4, Root, 2, CloseGroup, Sum, OpenGroup, 3, Factor, 2, CloseGroup],
                        evaluationResult: 8
                    ),
                    new FormulaTestCase("Exponentiation with parentheses",
                        formula: "2**(3+1)",
                        linqExpr: "(2 ** (3 + 1))",
                        syntheticTokens: [2, Exp, OpenGroup, 3, Sum, 1, CloseGroup],
                        evaluationResult: 16
                    ),
                    new FormulaTestCase("Complex",
                        formula: "(2 + 3) ** 4 // (3 -1)",
                        linqExpr: "((2 + 3) ** (4 ** (1 / (3 - 1))))",
                        syntheticTokens: [OpenGroup, 2, Sum, 3, CloseGroup, Exp, 4, Root, OpenGroup, 3, Subtract, 1, CloseGroup],
                        evaluationResult: 25
                    ),
                    #endregion Priorities

                    #region Function calls
                    new FormulaTestCase("Call Tick",
                        formula: "TICK()",
                        linqExpr: "Tick()",
                        syntheticTokens: ["TICK", OpenGroup, CloseGroup],
                        evaluationResult: 0
                    ),
                    new FormulaTestCase("Call Average on 1 value",
                        formula: "AVG(42)",
                        linqExpr: "new [] {42}.Average()",
                        syntheticTokens: ["AVG", OpenGroup, 42, CloseGroup],
                        evaluationResult: 42
                    ),
                    new FormulaTestCase("Call Average on 2 values",
                        formula: "AVG(1, 3)",
                        linqExpr: "new [] {1, 3}.Average()",
                        syntheticTokens: ["AVG", OpenGroup, 1, ArgSep, 3, CloseGroup],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Call Average on 3 values",
                        formula: "AVG(1, 3, 5)",
                        linqExpr: "new [] {1, 3, 5}.Average()",
                        syntheticTokens: ["AVG", OpenGroup, 1, ArgSep, 3, ArgSep, 5, CloseGroup],
                        evaluationResult: 3
                    ),
                    new FormulaTestCase("Nested Average call on 2 values",
                        formula: "AVG(AVG(0, 2), 3)",
                        linqExpr: "new [] {new [] {0, 2}.Average(), 3}.Average()",
                        syntheticTokens: ["AVG", OpenGroup, "AVG", OpenGroup, 0, ArgSep, 2, CloseGroup, ArgSep, 3, CloseGroup],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Call Average on 2 values with expressions within",
                        formula: "AVG(1 + 1, 3+2-1)",
                        linqExpr: "new [] {(1 + 1), ((3 + 2) - 1)}.Average()",
                        syntheticTokens: ["AVG", OpenGroup, 1, Sum, 1, ArgSep, 3, Sum, 2, Subtract, 1, CloseGroup],
                        evaluationResult: 3
                    ),
                    new FormulaTestCase("Call SQRT",
                        formula: "SQRT(4)",
                        linqExpr: "Sqrt(4)",
                        syntheticTokens: ["SQRT", OpenGroup, 4, CloseGroup],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Call ROOT",
                        formula: "ROOT(16, 4)",
                        linqExpr: "(16 ** (1 / 4))",
                        syntheticTokens: ["ROOT", OpenGroup, 16, ArgSep, 4, CloseGroup],
                        evaluationResult: 2
                    ),
                    new FormulaTestCase("Call MIN",
                        formula: "MIN(1, 42, -1, 2)",
                        linqExpr: "new [] {1, 42, -1, 2}.Min()",
                        syntheticTokens: ["MIN", OpenGroup, 1, ArgSep, 42, ArgSep, Subtract, 1, ArgSep, 2, CloseGroup],
                        evaluationResult: -1
                    ),
                    new FormulaTestCase("Call MAX",
                        formula: "MAX(1, 42, -1, 2)",
                        linqExpr: "new [] {1, 42, -1, 2}.Max()",
                        syntheticTokens: ["MAX", OpenGroup, 1, ArgSep, 42, ArgSep, Subtract, 1, ArgSep, 2, CloseGroup],
                        evaluationResult: 42
                    ),
                    #endregion
                ];
                bindingsTestCases = [
                    #region vars binding
                    new FormulaTestCase("Bind 1 variable",
                        formula: "response",
                        linqExpr: "response(pawn)",
                        syntheticTokens: ["response"],
                        evaluationResult: 42,
                        bindings: new(){{"response", (_,_,_) => 42}}
                    ),
                    new FormulaTestCase("Bind 2 variable",
                        formula: "response + bestTime",
                        linqExpr: "(response(pawn) + bestTime(pawn))",
                        syntheticTokens: ["response", Sum, "bestTime"],
                        evaluationResult: 42 + 420,
                        bindings: new(){ { "response", (_, _, _) => 42 }, {"bestTime", (_,_,_) => 420} }
                    ),
                    #endregion
                ];
                setsMap = new()
                {
                    { "basic", basicTestCases },
                    { "bindings", bindingsTestCases }
                };
            }

            private static IEnumerable<object> NormalizeSyntheticTokens(
                IEnumerable<object> objects
            ) =>
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

            private static IEnumerable<IToken> TokensFromSynthetic(IEnumerable<object> objects) =>
                NormalizeSyntheticTokens(objects)
                    .Select<object, IToken>(token =>
                        token switch
                        {
                            string name => new NameToken(name),
                            double num => new NumberToken(num),
                            Operator op => new OperatorToken(op),
                            _ => throw new InvalidCastException(),
                        }
                    );

            private static IEnumerable<object> SyntheticFromTokens(IEnumerable<IToken> tokens) =>
                tokens.Select<IToken, object>(token =>
                    token switch
                    {
                        NameToken t => t.Value,
                        NumberToken t => t.Value,
                        OperatorToken t => t.Value,
                        _ => throw new InvalidCastException(),
                    }
                );

            public static IEnumerable<object[]> GetTokenizeFormulaData(params string[] sets) =>
                sets.SelectMany(set => setsMap[set]).Select(test =>
                    new object[] { test.Description, test.Formula, test.SyntheticTokens }
                );

            [Theory, MemberData(nameof(GetTokenizeFormulaData), parameters: [new[] { "basic", "bindings" }])]
            public void ShouldTokenizeValidFormula(
                string description,
                string formula,
                object[] expected
            )
            {
                var returned = TokenizeFormula(formula);
                Assert.Equal(NormalizeSyntheticTokens(expected), SyntheticFromTokens(returned));
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
                    () => TokenizeFormula(formula).ToList()
                );
            }

            public static IEnumerable<object[]> GetParseTokensData(params string[] sets) =>
                sets.SelectMany(set => setsMap[set]).Select(test =>
                    new object[] { test.Description, test.SyntheticTokens, test.EvaluationResult }
                );

            [Theory, MemberData(nameof(GetParseTokensData), parameters: [new[] { "basic", "bindings" }])]
            public void ShouldParseTokens(string description, object[] tokens, float expected)
            {
                var context = new FormulaPawnFitness.Parser.Context();
                var bindings = GetTestBindings(description);
                var result = context.ParseTokens(ToAst(TokensFromSynthetic(tokens)));
                Assert.Equal(expected, result.Calc(null, null, null, bindings));
            }

            [
                Theory,
                InlineData(new object[] { }, typeof(ArgumentOutOfRangeException)), // Should have a better type and a clear message
                InlineData(new object[] { OpenGroup }, typeof(ArgumentOutOfRangeException)), // Should have a better type and a clear message
                InlineData(new object[] { "TICK", OpenGroup, 1, CloseGroup }, typeof(InvalidOperationException), "Bad arity for function TICK, expected 0 parameters, have 1"),
                InlineData(new object[] { "MIN", OpenGroup, CloseGroup }, typeof(InvalidOperationException), "Bad arity for function MIN, expected at least 1 parameters, have 0"),
            ]
            public void ShouldFailOnParse(
                object[] tokens,
                Type expectedError,
                string? message = null
            )
            {
                var context = new Context();
                var exception = Assert.Throws(
                    expectedError,
                    () => context.ParseTokens(ToAst(TokensFromSynthetic(tokens)))
                );
                if (message != null)
                {
                    Assert.Equal(message, exception.Message);
                }
            }

            public static IEnumerable<object[]> GetEvaluateFormulaData(params string[] sets) =>
                sets.SelectMany(set => setsMap[set]).Select(test =>
                    new object[] { test.Description, test.Formula, test.EvaluationResult }
                );

            [Theory, MemberData(nameof(GetEvaluateFormulaData), parameters: [new[] { "basic", "bindings" }])]
            public void ShouldEvaluateFormula(string description, string formula, float expected)
            {
                var formulaExpression = new FormulaPawnFitness.Parser().ParseFormula(formula);
                var bindings = GetTestBindings(description);
                var result = formulaExpression.Calc(null, null, null, bindings);
                Assert.Equal(expected, result);
            }

            public static IEnumerable<object[]> GetFormulaToExprData(params string[] sets) =>
                sets.SelectMany(set => setsMap[set]).Select(test =>
                    new object[] { test.Description, test.Formula, test.LinqExpr }
                );

            [Theory, MemberData(nameof(GetFormulaToExprData), parameters: [new[] { "basic" }])]
            public void ShouldFormulaToExpr(string description, string formula, string expected)
            {
                var formulaExpression = new FormulaPawnFitness.Parser().ParseFormula(formula);
                var linqExpr = formulaExpression.Expression.ToString();
                Assert.Equal($"(pawn, specification, request, bindings) => {expected}", linqExpr);
            }
        }
    }
}
