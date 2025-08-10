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
            public delegate double CalcFitnessFunction(
                Pawn pawn,
                WorkSpecification specification,
                ResolveWorkRequest request
            );
            public delegate double UnboundCalcFitnessFunction(
                Pawn pawn,
                WorkSpecification specification,
                ResolveWorkRequest request,
                FormulaBindings? bindings
            );

            public class FormulaBindings : Dictionary<string, CalcFitnessFunction>
            {
                public FormulaBindings() { }

                public FormulaBindings(IDictionary<string, CalcFitnessFunction> source)
                {
                    this.AddRange(source);
                }

                public void Validate(string[] declaredNames)
                {
                    if (!new HashSet<string>(declaredNames).SetEquals(Keys))
                    {
                        throw new Exception("Invalid bindings");
                    }
                }
            }

            public class Formula
            {
                public string[] BindingNames { get; private set; }

                public Formula(
                    Expression<UnboundCalcFitnessFunction> expression,
                    string[] bindingNames
                )
                {
                    Expression = expression;
                    BindingNames = bindingNames;
                }

                public Expression<UnboundCalcFitnessFunction> Expression { get; }

                public CalcFitnessFunction Bind(FormulaBindings bindings)
                {
                    bindings.Validate(BindingNames);
                    var fn = Expression.Compile();
                    return (pawn, specification, request) =>
                        fn(pawn, specification, request, bindings);
                }

                public float Calc(
                    Pawn pawn,
                    WorkSpecification specification,
                    ResolveWorkRequest request,
                    FormulaBindings bindings
                )
                {
                    bindings.Validate(BindingNames);
                    var fn = Expression.Compile();
                    return (float)fn(pawn, specification, request, bindings);
                }
            }

            internal class Context
            {
                private static readonly ParameterExpression pawnParameter = Expression.Parameter(
                    typeof(Pawn),
                    "pawn"
                );
                private static readonly ParameterExpression specificationParameter =
                    Expression.Parameter(typeof(WorkSpecification), "specification");
                private static readonly ParameterExpression requestParameter = Expression.Parameter(
                    typeof(ResolveWorkRequest),
                    "request"
                );
                private static readonly ParameterExpression bindingsParameter =
                    Expression.Parameter(typeof(FormulaBindings), "bindings");
                private static readonly ParameterExpression[] parameters = new[]
                {
                    pawnParameter,
                    specificationParameter,
                    requestParameter,
                    bindingsParameter,
                };

                private class BindingExpressionReplacer : ExpressionVisitor
                {
                    private readonly Dictionary<ParameterExpression, ParameterExpression> mapping;

                    public BindingExpressionReplacer(
                        Expression<UnboundCalcFitnessFunction> sourceExpression
                    )
                    {
                        mapping = sourceExpression
                            .Parameters.Zip(
                                parameters,
                                (source, replacement) =>
                                    new KeyValuePair<ParameterExpression, ParameterExpression>(
                                        source,
                                        replacement
                                    )
                            )
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    }

                    public override Expression Visit(Expression node)
                    {
                        if (
                            node is ParameterExpression nodeParam
                            && mapping.TryGetValue(nodeParam, out var replacement)
                        )
                        {
                            return replacement;
                        }
                        return base.Visit(node);
                    }
                }

                static double Tick()
                {
                    return 0;
                }

                internal Formula ParseTokens(AstNode ast)
                {
                    var queue = new List<AstNode>(new[] { ast });

                    // Collect all, deep first
                    for (var i = 0; i < queue.Count; i++)
                    {
                        var cursor = queue[i];
                        switch (cursor)
                        {
                            case AstLiteral:
                                break;
                            case AstBindingExpression:
                                break;
                            case AstCallExpression call:
                                queue.AddRange(call.Children);
                                break;
                            case IAstExpression expr:
                                // Flatten empty groups (eg: `(1)`, `(-4.2)`, `(foo())`
                                if (
                                    expr.Children.Count() == 1
                                    && expr is not AstArithmeticExpression
                                )
                                {
                                    queue.Remove(cursor);
                                    i--;
                                }
                                queue.AddRange(expr.Children.Cast<AstNode>());
                                break;

                            default:
                                throw new ArgumentException(
                                    $"Invalid token type {cursor.GetType().Name}",
                                    nameof(cursor)
                                );
                        }
                    }

                    queue.Reverse();
                    var npnExpr = new Dictionary<AstNode, Expression>();
                    var bindings = new HashSet<string>();
                    Expression last = null;
                    foreach (var cursor in queue)
                    {
                        switch (cursor)
                        {
                            case AstLiteral literal:
                                last = Expression.Constant(literal.Token.Value, typeof(double));
                                break;

                            case AstUnaryExpression unaryExpr:
                                {
                                    if (unaryExpr.Operator == Operator.Subtract)
                                    {
                                        last = Expression.Negate(
                                            npnExpr[unaryExpr.Child]
                                        );
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException(
                                            $"Operator {Enum.GetName(typeof(Operator), unaryExpr.Token.Value)} not supported at this point"
                                        );
                                    }
                                }
                                break;

                            case AstBinaryExpression binaryExpr:
                                {
                                    switch (binaryExpr.Token.Value)
                                    {
                                        case Operator.Exp:
                                            last = Expression.Power(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Root:
                                            last = null;
                                            if (binaryExpr.Children[1] is AstLiteral lit)
                                            {
                                                if (lit.Token.Value == 2)
                                                {
                                                    last = Expression.Call(
                                                        ((Func<double, double>)Math.Sqrt).Method,
                                                        npnExpr[binaryExpr.Children[0]]
                                                    );
                                                }
                                                else if (lit.Token.Value == 3)
                                                {
                                                    // Not supported in that dotnet version
                                                    // root = Expression.Call(((Func<double, double>)Math.Cbrt).Method, npnExpr[anyOperator.Children[0]]);
                                                }
                                            }
                                            last ??= Expression.Power(
                                                npnExpr[binaryExpr.Children[0]],
                                                Expression.Divide(
                                                    Expression.Constant((double)1, typeof(double)),
                                                    npnExpr[binaryExpr.Children[1]]
                                                )
                                            );
                                            break;
                                        case Operator.Factor:
                                            last = Expression.Multiply(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Divide:
                                            last = Expression.Divide(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Sum:
                                            last = Expression.Add(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Subtract:
                                            last = Expression.Subtract(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Modulus:
                                            last = Expression.Modulo(
                                                npnExpr[binaryExpr.Children[0]],
                                                npnExpr[binaryExpr.Children[1]]
                                            );
                                            break;
                                        default:
                                            throw new InvalidOperationException(
                                                $"Operator {Enum.GetName(typeof(Operator), binaryExpr.Token.Value)} not supported at this point"
                                            );
                                    }
                                }
                                break;

                            case AstCallExpression call:
                                {
                                    var callParams = call
                                        .Children.Select(child => npnExpr[child])
                                        .ToList();
                                    switch (call.Token.Value)
                                    {
                                        case "AVG":
                                            last = Expression.Call(
                                                (
                                                    (Func<IEnumerable<double>, double>)
                                                        Enumerable.Average
                                                ).Method,
                                                Expression.NewArrayInit(typeof(double), callParams)
                                            );
                                            break;

                                        case "TICK":
                                            last = Expression.Call(((Func<double>)Tick).Method);
                                            break;

                                        case "ROOT":
                                            if (callParams.Count != 2)
                                            {
                                                throw new InvalidOperationException(
                                                    "Bad call signature"
                                                );
                                            }
                                            last = Expression.Power(
                                                callParams[0],
                                                Expression.Divide(
                                                    Expression.Constant((double)1, typeof(double)),
                                                    callParams[1]
                                                )
                                            );
                                            break;

                                        case "SQRT":
                                            last = Expression.Call(
                                                ((Func<double, double>)Math.Sqrt).Method,
                                                callParams.Single()
                                            );
                                            break;

                                        case "MIN":
                                            last = Expression.Call(
                                                (
                                                    (Func<IEnumerable<double>, double>)
                                                        Enumerable.Min
                                                ).Method,
                                                Expression.NewArrayInit(typeof(double), callParams)
                                            );
                                            break;

                                        case "MAX":
                                            last = Expression.Call(
                                                (
                                                    (Func<IEnumerable<double>, double>)
                                                        Enumerable.Max
                                                ).Method,
                                                Expression.NewArrayInit(typeof(double), callParams)
                                            );
                                            break;

                                        default:
                                            throw new NotImplementedException(
                                                $"Method call \"{call.Token.Value}\" is not implemented"
                                            );
                                    }
                                }
                                break;

                            case AstBindingExpression binding:
                                {
                                    bindings.Add(binding.Name);
                                    Expression<UnboundCalcFitnessFunction> expr = (
                                        pawn,
                                        specification,
                                        request,
                                        bindings
                                    ) => bindings[binding.Name](pawn, specification, request);
                                    var replacer = new BindingExpressionReplacer(expr);
                                    last = replacer.Visit(expr.Body);
                                }
                                break;

                            default:
                                throw new ArgumentException(
                                    $"Invalid token type {cursor.GetType().Name}",
                                    nameof(cursor)
                                );
                        }
                        npnExpr.SetOrAdd(cursor, last);
                    }

                    if (last == null)
                    {
                        throw new InvalidOperationException("No tokens");
                    }
                    var callExpr = Expression.Lambda<UnboundCalcFitnessFunction>(
                        last,
                        pawnParameter,
                        specificationParameter,
                        requestParameter,
                        bindingsParameter
                    );
                    return new Formula(callExpr, bindings.ToArray());
                }
            }

            public Formula ParseFormula(string source)
            {
                var tokens = TokenizeFormula(source);
                var ast = ToAst(tokens);
                var context = new Context();
                var result = context.ParseTokens(ast);
                return result;
            }
        }
    }
}
