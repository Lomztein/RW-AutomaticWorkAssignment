using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness
    {
        public delegate double CalcFitnessFunction(
            Pawn pawn,
            WorkSpecification specification,
            ResolveWorkRequest request
        );
        internal delegate double UnboundCalcFitnessFunction(
            Pawn pawn,
            WorkSpecification specification,
            ResolveWorkRequest request,
            Formula.Bindings? bindings
        );
        public class Formula
        {
            public class Bindings : Dictionary<string, CalcFitnessFunction>
            {
                public Bindings() { }

                public Bindings(
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
                    )
                { }

                public Bindings(IDictionary<string, CalcFitnessFunction> source)
                {
                    this.AddRange(source);
                }

                public void Validate(string[] declaredNames)
                {
                    if (!new HashSet<string>(declaredNames).SetEquals(Keys))
                    {
                        throw new InvalidOperationException("Invalid bindings");
                    }
                }
            }

            public string[] BindingNames { get; private set; }

            internal Formula(
                Expression<UnboundCalcFitnessFunction> expression,
                string[] bindingNames
            )
            {
                Expression = expression;
                BindingNames = bindingNames;
            }

            internal Expression<UnboundCalcFitnessFunction> Expression { get; }

            public CalcFitnessFunction Bind(Bindings bindings)
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
                Bindings bindings
            )
            {
                bindings.Validate(BindingNames);
                var fn = Expression.Compile();
                return (float)fn(pawn, specification, request, bindings);
            }
        }

        public class ParseException : Exception
        {
            public override string Message
            {
                get
                {
                    if (InnerException is ParseException ex)
                    {
                        return ex.Message;
                    }
                    return base.Message;
                }
            }
            public ParseException() { }
            public ParseException(string message, Exception? innerException = null) : base(message, innerException) { }

        }
        internal partial class Parser
        {
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
                    Expression.Parameter(typeof(Formula.Bindings), "bindings");
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

                static class WellKnownFunctions
                {
                    public delegate Expression ExpressionFactory(params Expression[] args);

                    [AttributeUsage(AttributeTargets.Method)]
                    public class ArityAttribute : Attribute
                    {
                        private int? expected = default;
                        private int? min = default;
                        private int? max = default;
                        public int Min { get => min ?? expected.Value; set => min = value; }
                        public int Max { get => max ?? expected.Value; set => max = value; }
                        public ArityAttribute(int expected)
                        {
                            this.expected = expected;
                        }
                        public ArityAttribute() { }

                        public void CheckArity(string name, Expression[] callParams)
                        {
                            if (expected.HasValue)
                            {
                                if (min.HasValue || max.HasValue)
                                {
                                    throw new ArgumentException("Cannot provide an expected and (min | max) arity", nameof(expected));
                                }
                                if (callParams.Length != expected.Value)
                                {
                                    throw new ParseException($"Bad arity for function {name}, expected {expected.Value} parameters, have {callParams.Length}");
                                }
                                return;
                            }
                            string rangeMessage;
                            if (min.HasValue && max.HasValue)
                            {
                                rangeMessage = $"between {min.Value} and {max.Value}";
                            }
                            else if (min.HasValue)
                            {
                                rangeMessage = $"at least {min.Value}";
                            }
                            else if (max.HasValue)
                            {
                                rangeMessage = $"at most {max.Value}";
                            }
                            else
                            {
                                throw new InvalidOperationException("Bad arity options");
                            }

                            var localMin = min ?? 0;
                            var localMax = max ?? (int.MaxValue - 1);
                            if (callParams.Length < localMin || callParams.Length > max)
                            {
                                throw new ParseException($"Bad arity for function {name}, expected {rangeMessage} parameters, have {callParams.Length}");
                            }
                        }
                    }

                    public static (ExpressionFactory expressionFactory, ArityAttribute arity) LoadMethod(Delegate type)
                    {
                        var infos = type.GetMethodInfo();
                        var arityAttr = infos.GetCustomAttributes<ArityAttribute>().SingleOrDefault();
                        arityAttr ??= new ArityAttribute(infos.GetParameters().Length);
                        Expression factory(Expression[] callParams) => Expression.Call(type.Method, callParams);
                        return (factory, arityAttr);

                    }

                    #region Custom functions
                    internal static double Tick()
                    {
                        return 0;
                    }
                    #endregion

                    #region Standard functions
                    [Arity(1)]
                    internal static Expression SqrtExpression(params Expression[] callParams)
                    {
                        var value = callParams[0];
                        return Expression.Call(((Func<double, double>)Math.Sqrt).Method, value);
                    }

                    [Arity(2)]
                    internal static Expression RootExpression(params Expression[] callParams)
                    {
                        var left = callParams[0];
                        var right = callParams[1];
                        return Expression.Power(
                            left,
                            Expression.Divide(Expression.Constant((double)1, typeof(double)), right)
                        );
                    }

                    [Arity(Min = 1)]
                    internal static Expression MaxExpression(params Expression[] callParams) =>
                        Expression.Call(
                            ((Func<IEnumerable<double>, double>)Enumerable.Max).Method,
                            Expression.NewArrayInit(typeof(double), callParams)
                        );

                    [Arity(Min = 1)]
                    internal static Expression MinExpression(params Expression[] callParams) =>
                        Expression.Call(
                            ((Func<IEnumerable<double>, double>)Enumerable.Min).Method,
                            Expression.NewArrayInit(typeof(double), callParams)
                        );

                    [Arity(Min = 1)]
                    internal static Expression AvgExpression(params Expression[] callParams) =>
                        Expression.Call(
                            ((Func<IEnumerable<double>, double>)Enumerable.Average).Method,
                            Expression.NewArrayInit(typeof(double), callParams)
                        );

                    [Arity(3)]
                    internal static Expression ClampExpression(params Expression[] callParams)
                    {
                        var value = callParams[0];
                        var min = callParams[1];
                        var max = callParams[2];
                        return Expression.IfThenElse(
                            Expression.GreaterThan(value, max),
                            max,
                            Expression.IfThenElse(
                                Expression.LessThan(value, min),
                                min,
                                value
                            )
                        );
                    }
                    #endregion
                }

                static readonly Dictionary<string, WellKnownFunctions.ExpressionFactory> wellKnownFunctions = new Dictionary<string, WellKnownFunctions.ExpressionFactory>()
                {
                    {"SQRT", WellKnownFunctions.SqrtExpression},
                    {"ROOT", WellKnownFunctions.RootExpression},
                    {"MAX", WellKnownFunctions.MaxExpression},
                    {"MIN", WellKnownFunctions.MinExpression},
                    {"AVG", WellKnownFunctions.AvgExpression},
                    {"CLAMP", WellKnownFunctions.ClampExpression},
                }
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => (
                            expressionFactory: kvp.Value,
                            arity: kvp.Value.GetMethodInfo().GetCustomAttributes<WellKnownFunctions.ArityAttribute>().Single()))
                .Union(new Dictionary<string, (WellKnownFunctions.ExpressionFactory expressionFactory, WellKnownFunctions.ArityAttribute arity)>()
                {
                    {"TICK", WellKnownFunctions.LoadMethod((Func<double>)WellKnownFunctions.Tick) },
                }).ToDictionary(
                    kvp => kvp.Key,
                    kvp => (WellKnownFunctions.ExpressionFactory)((callParams) =>
                    {
                        kvp.Value.arity.CheckArity(kvp.Key, callParams);
                        return kvp.Value.expressionFactory(callParams);
                    }));


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
                                                    last = wellKnownFunctions["SQRT"](
                                                        astExprMap[binaryExpr.Children[0]]
                                                    );
                                                }
                                                else if (lit.Token.Value == 3)
                                                {
                                                    // Not supported in that dotnet version
                                                    // root = Expression.Call(((Func<double, double>)Math.Cbrt).Method, npnExpr[anyOperator.Children[0]]);
                                                }
                                            }
                                            last ??= wellKnownFunctions["ROOT"](
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
                                        .ToArray();

                                    if (wellKnownFunctions.TryGetValue(call.Name, out var fn))
                                    {
                                        last = fn(callParams);
                                    }
                                    else
                                    {
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
                try
                {
                    var tokens = TokenizeFormula(source);
                    var ast = ToAst(tokens);
                    var context = new Context();
                    var result = context.ParseTokens(ast);
                    return result;
                }
                catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is InvalidOperationException || ex is ParseException)
                {
                    throw new ParseException("Formula is invalid", ex);
                }
            }
        }
    }
}
