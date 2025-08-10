using RimWorld;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Verse;
using static Lomzie.AutomaticWorkAssignment.PawnFitness.FormulaPawnFitness.Parser;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness
    {
        internal class Parser
        {
            #region Tokenize
            internal interface IToken { }
            internal enum Operator
            {
                OpenGroup,
                CloseGroup,
                ArgSep,
                Sum,
                Subtract,
                Factor,
                Divide,
                Modulus,
                Exp,
                Root,
            }
            private static readonly Dictionary<char, Operator> _singleCharOperatorDict = new()
            {
                {'(', Operator.OpenGroup},
                {')', Operator.CloseGroup},
                {',', Operator.ArgSep},
                {'+', Operator.Sum},
                {'-', Operator.Subtract},
                {'*', Operator.Factor},
                {'/', Operator.Divide},
                {'%', Operator.Modulus},
            };
            internal readonly struct NumberToken : IToken
            {
                public NumberToken(double value)
                {
                    Value = value;
                }
                public readonly double Value;
            }
            internal readonly struct NameToken : IToken
            {
                public NameToken(string value)
                {
                    Value = value;
                }
                public readonly string Value;
            }
            internal readonly struct OperatorToken : IToken
            {
                public OperatorToken(Operator value)
                {
                    Value = value;
                }
                public readonly Operator Value;
            }
            internal static IEnumerable<IToken> TokenizeFormula(string formula)
            {
                int i = 0;


                bool IsValidNameChar(bool leading)
                {
                    return i < formula.Length && (char.IsLetter(formula[i]) || formula[i] == '_' || (!leading && char.IsNumber(formula[i])));
                }
                bool IsMulticharOperator(string op)
                {
                    return (i + (op.Length - 1)) < formula.Length && formula.Substring(i, op.Length) == op;
                }

                while (i < formula.Length)
                {
                    // Skip whitespace
                    if (char.IsWhiteSpace(formula[i]))
                    {
                        i++;
                        continue;
                    }

                    // Handle digits
                    if (char.IsDigit(formula[i]) || formula[i] == '.')
                    {
                        int start = i;
                        bool hasDecimalSep = formula[i] == '.';
                        while (i < formula.Length && (char.IsDigit(formula[i]) || formula[i] == '_' || formula[i] == '.'))
                        {
                            if (formula[i] == '.')
                            {
                                if (hasDecimalSep)
                                {
                                    throw new ArgumentException($"Invalid number: '{formula[i]}' (formula is \"{formula}\", invalid at index {i})", nameof(formula));
                                }
                                hasDecimalSep = true;
                            }
                            i++;
                        }
                        if (IsValidNameChar(leading: false))
                        {
                            throw new ArgumentException($"Invalid number: '{formula[i]}' (formula is \"{formula}\", invalid at index {i})", nameof(formula));
                        }
                        string s = formula.Substring(start, i - start).Replace("_", "");
                        yield return new NumberToken(double.Parse(s, CultureInfo.InvariantCulture));
                    }
                    // Handle multi-char operators
                    else if (IsMulticharOperator("**"))
                    {
                        yield return new OperatorToken(Operator.Exp);
                        i += 2;
                        continue;
                    }
                    else if (IsMulticharOperator("//"))
                    {
                        yield return new OperatorToken(Operator.Root);
                        i += 2;
                        continue;
                    }
                    // Handle operators
                    else if (_singleCharOperatorDict.TryGetValue(formula[i], out var op))
                    {
                        yield return new OperatorToken(op);
                        i++;
                    }
                    // Handle name (function calls or vars)
                    else if (IsValidNameChar(leading: true))
                    {
                        int start = i;
                        while (i < formula.Length && (IsValidNameChar(leading: false)))
                        {
                            i++;
                        }
                        var name = formula.Substring(start, i - start);

                        yield return new NameToken(name);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid character: '{formula[i]}' (formula is \"{formula}\", invalid at index {i})", nameof(formula));
                    }
                }
            }
            #endregion

            #region Parse
            interface IExpression { }
            readonly struct LiteralExpression : IExpression
            {
                public readonly double Value;
            }
            readonly struct FunctionCallExpression : IExpression
            {
                public readonly string Name;
                public readonly IExpression[] Arguments;
            }
            readonly struct ArgumentExpression : IExpression
            {
                public readonly string Name;
            }
            readonly struct BinaryOperationExpression: IExpression
            {
                public readonly IExpression Left;
                public readonly IExpression Right;
            }
            public delegate double CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request);
            public class Formula {
                public Formula(Expression<CalcFitness> expression)
                {
                    Expression = expression;
                }

                public Expression<CalcFitness> Expression { get; }
                public float Calc(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
                {
                    var fn = Expression.Compile();
                    return (float)fn(pawn, specification, request);
                }
            }
            internal class Context
            {
                private static readonly ParameterExpression pawnParameter = Expression.Parameter(typeof(Pawn), "pawn");
                private static readonly ParameterExpression specificationParameter = Expression.Parameter(typeof(WorkSpecification), "specification");
                private static readonly ParameterExpression requestParameter = Expression.Parameter(typeof(ResolveWorkRequest), "request");
                private Expression? root;

                internal Context ParseTokens(IEnumerable<IToken> tokens)
                {
                    Expression parent = null;
                    Action<Expression> yield = prev =>
                    {
                        parent = prev;
                    };

                    foreach (var current in tokens)
                    {
                        switch (current)
                        {
                            case NumberToken numberToken:
                                var expr = Expression.Constant(numberToken.Value, typeof(double));
                                yield(expr);
                                yield = null;
                                break;

                            case OperatorToken operatorToken:
                                Expression currentPrev = parent;
                                switch (operatorToken.Value)
                                {
                                    case Operator.Factor:
                                        yield = (next) =>
                                        {
                                            if (next == null)
                                            {
                                                throw new InvalidOperationException("prev");
                                            }
                                            parent = Expression.Multiply(currentPrev, next);
                                        };
                                        break;
                                    case Operator.Sum:
                                        yield = (next) =>
                                        {
                                            if (next == null)
                                            {
                                                throw new InvalidOperationException("prev");
                                            }
                                            Expression.Add(currentPrev, next);
                                            parent = Expression.Add(currentPrev, next);
                                        };
                                        break;
                                    default:
                                        throw new NotImplementedException($"Operator {Enum.GetName(typeof(Operator), operatorToken.Value)} not supported");
                                }
                                break;

                            default:
                                throw new ArgumentException("Invalid token type", nameof(current));

                        }
                    }
                    root = parent;
                    return this;
                }

                internal Formula ToFormula()
                {
                    if (root == null)
                    {
                        throw new InvalidOperationException("No tokens");
                    }
                    var body = root;
                    var callExpr = Expression.Lambda<CalcFitness>(body, pawnParameter, specificationParameter, requestParameter);
                    return new Formula(callExpr);
                }
            }
            #endregion
        }
    }
}
