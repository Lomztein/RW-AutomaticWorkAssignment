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

                public FormulaBindings(
                    IDictionary<
                        string,
                        Func<Pawn, WorkSpecification, ResolveWorkRequest, float>
                    > source
                )
                    : this(
                        source.ToDictionary(
                            kvp => kvp.Key,
                            kvp =>
                                (CalcFitnessFunction)(
                                    (pawn, specification, request) =>
                                        (double)kvp.Value(pawn, specification, request)
                                )
                        )
                    ) { }

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

                #region Custom functions
                static double Tick()
                {
                    return 0;
                }
                #endregion
                #region Standard functions
                Expression SqrtExpression(Expression value) =>
                    Expression.Call(((Func<double, double>)Math.Sqrt).Method, value);

                private static BinaryExpression RootExpression(Expression left, Expression right) =>
                    Expression.Power(
                        left,
                        Expression.Divide(Expression.Constant((double)1, typeof(double)), right)
                    );

                private static MethodCallExpression MaxExpression(List<Expression> callParams) =>
                    Expression.Call(
                        ((Func<IEnumerable<double>, double>)Enumerable.Max).Method,
                        Expression.NewArrayInit(typeof(double), callParams)
                    );

                private static MethodCallExpression MinExpression(List<Expression> callParams) =>
                    Expression.Call(
                        ((Func<IEnumerable<double>, double>)Enumerable.Min).Method,
                        Expression.NewArrayInit(typeof(double), callParams)
                    );

                private static MethodCallExpression AvgExpression(List<Expression> callParams) =>
                    Expression.Call(
                        ((Func<IEnumerable<double>, double>)Enumerable.Average).Method,
                        Expression.NewArrayInit(typeof(double), callParams)
                    );
                #endregion

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
                    var astExprMap = new Dictionary<AstNode, Expression>();
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
                                        last = Expression.Negate(astExprMap[unaryExpr.Child]);
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
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Root:
                                            last = null;
                                            if (binaryExpr.Children[1] is AstLiteral lit)
                                            {
                                                if (lit.Token.Value == 2)
                                                {
                                                    last = SqrtExpression(
                                                        astExprMap[binaryExpr.Children[0]]
                                                    );
                                                }
                                                else if (lit.Token.Value == 3)
                                                {
                                                    // Not supported in that dotnet version
                                                    // root = Expression.Call(((Func<double, double>)Math.Cbrt).Method, npnExpr[anyOperator.Children[0]]);
                                                }
                                            }
                                            last ??= RootExpression(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Factor:
                                            last = Expression.Multiply(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Divide:
                                            last = Expression.Divide(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Sum:
                                            last = Expression.Add(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Subtract:
                                            last = Expression.Subtract(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
                                            );
                                            break;
                                        case Operator.Modulus:
                                            last = Expression.Modulo(
                                                astExprMap[binaryExpr.Children[0]],
                                                astExprMap[binaryExpr.Children[1]]
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
                                        .Children.Select(child => astExprMap[child])
                                        .ToList();
                                    switch (call.Token.Value)
                                    {
                                        case "AVG":
                                            last = AvgExpression(callParams);
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
                                            if (
                                                callParams[1] is ConstantExpression constExpr
                                                && (double)constExpr.Value == 2
                                            )
                                            {
                                                last = SqrtExpression(callParams[0]);
                                            }
                                            else
                                            {
                                                last = RootExpression(callParams[0], callParams[1]);
                                            }
                                            break;

                                        case "SQRT":
                                            last = SqrtExpression(callParams.Single());
                                            break;

                                        case "MIN":
                                            last = MinExpression(callParams);
                                            break;

                                        case "MAX":
                                            last = MaxExpression(callParams);
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
                        astExprMap.SetOrAdd(cursor, last);
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
