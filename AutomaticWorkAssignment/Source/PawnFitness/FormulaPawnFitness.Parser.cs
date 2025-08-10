using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness
    {
        internal partial class Parser
        {
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

                static double Tick()
                {
                    return 0;
                }

                internal Context ParseTokens(IEnumerable<IToken> tokens)
                {                    
                    var ast = ToAst(tokens);
                    var queue = new List<AstNode>(new[] { ast });

                    // Collect all, deep first
                    for(var i = 0; i < queue.Count; i++) {
                        var cursor = queue[i];
                        switch (cursor)
                        {
                            case AstLiteral literal:
                                break;
                            case AstCallExpression call:
                                queue.AddRange(call.Children);
                                break;
                            case IAstExpression expr:
                                // Flatten empty groups (eg: `(1)`, `(-4.2)`, `(foo())`
                                if (expr.Children.Count() == 1 && expr is not AstArithmeticExpression)
                                {
                                    queue.Remove(cursor);
                                    i--;
                                }
                                queue.AddRange(expr.Children.Cast<AstNode>());
                                break;

                            default:
                                throw new ArgumentException("Invalid token type", nameof(cursor));

                        }
                    }

                    queue.Reverse();
                    var npnExpr = new Dictionary<AstNode, Expression>();
                    foreach (var cursor in queue)
                    {
                        switch (cursor)
                        {
                            case AstLiteral literal:
                                root = Expression.Constant(literal.Token.Value, typeof(double));
                                break;

                            case AstArithmeticExpression anyOperator:
                                switch (anyOperator.Token.Value)
                                {
                                    case Operator.Exp:
                                        root = Expression.Power(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Root:
                                        root = null;
                                        if (anyOperator.Children[1] is AstLiteral lit)
                                        {
                                            if (lit.Token.Value == 2)
                                            {
                                                root = Expression.Call(((Func<double, double>)Math.Sqrt).Method, npnExpr[anyOperator.Children[0]]);
                                            }
                                            else if (lit.Token.Value == 3)
                                            {
                                                // Not supported in that dotnet version
                                                // root = Expression.Call(((Func<double, double>)Math.Cbrt).Method, npnExpr[anyOperator.Children[0]]);
                                            }
                                        }
                                        if (root == null)
                                        {
                                            root = Expression.Power(
                                                npnExpr[anyOperator.Children[0]],
                                                Expression.Divide(
                                                    Expression.Constant((double)1, typeof(double)),
                                                    npnExpr[anyOperator.Children[1]]
                                                )
                                            );
                                        }
                                        break;
                                    case Operator.Factor:
                                        root = Expression.Multiply(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Divide:
                                        root = Expression.Divide(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Sum:
                                        root = Expression.Add(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Subtract:
                                        root = Expression.Subtract(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Modulus:
                                        root = Expression.Modulo(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    default:
                                        throw new NotImplementedException($"Operator {Enum.GetName(typeof(Operator), anyOperator.Token.Value)} not supported");
                                }
                                break;

                            case AstCallExpression call:
                                var callParams = call.Children.Select(child => npnExpr[child]).ToList();
                                switch (call.Token.Value)
                                {
                                    case "AVG":
                                        root = Expression.Call(((Func<IEnumerable<double>, double>)Enumerable.Average).Method, Expression.NewArrayInit(typeof(double), callParams));
                                        break;

                                    case "TICK":
                                        root = Expression.Call(((Func<double>)Tick).Method);
                                        break;

                                    case "ROOT":
                                        if (callParams.Count != 2)
                                        {
                                            throw new InvalidOperationException("Bad call signature");
                                        }
                                        root = Expression.Power(
                                            callParams[0],
                                            Expression.Divide(
                                                Expression.Constant((double)1, typeof(double)),
                                                callParams[1]
                                            )
                                        );
                                        break;

                                    case "SQRT":
                                        root = Expression.Call(((Func<double, double>)Math.Sqrt).Method, callParams.Single());
                                        break;

                                    case "MIN":
                                        root = Expression.Call(((Func<IEnumerable<double>, double>)Enumerable.Min).Method, Expression.NewArrayInit(typeof(double), callParams));
                                        break;

                                    case "MAX":
                                        root = Expression.Call(((Func<IEnumerable<double>, double>)Enumerable.Max).Method, Expression.NewArrayInit(typeof(double), callParams));
                                        break;

                                    default:
                                        throw new NotImplementedException($"Method call \"{call.Token.Value}\" is not implemented");
                                }
                                break;

                            default:
                                throw new ArgumentException($"Invalid token type {cursor.GetType().Name}", nameof(cursor));

                        }
                        npnExpr.SetOrAdd(cursor, root);
                    }
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

            public static Formula ParseFormula(string source)
            {
                var tokens = TokenizeFormula(source);
                var context = new Context();
                var result = context.ParseTokens(tokens).ToFormula();
                return result;
            }
        }
    }
}
