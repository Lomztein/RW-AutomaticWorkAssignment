using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            internal class OperatorOrderComparer : IComparer<Operator>
            {
                private static readonly (bool ltr, Operator[] operators)[] orders = new (bool ltr, Operator[] operators)[]
                {
                    // `2**3**2` ⇒ `2**(3**2)`
                    (ltr: false, operators: new Operator[] {
                        Operator.Exp,
                        Operator.Root,
                    }),
                    (ltr: true, new Operator[] {
                        Operator.Factor,
                        Operator.Divide,
                    }),
                    (ltr: true, new Operator[] {
                        Operator.Sum,
                        Operator.Subtract,
                    }),
                };
                static (int index, bool ltr, Operator[] operators) GetGroup(Operator x)
                {
                    return orders.Select((opGroup, index) => (index, opGroup.ltr, opGroup.operators)).Single(opGroup => opGroup.operators.Contains(x));
                }
                public int Compare(Operator x, Operator y)
                {
                    var xGroup = GetGroup(x);
                    var yGroup = GetGroup(y);
                    if (xGroup != yGroup)
                    {
                        return xGroup.index - yGroup.index;
                    }
                    return xGroup.ltr ? -1 : 1;
                }
            }
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

                interface IAstNode
                {
                    IAstExpression? Parent { get; }
                }
                interface IAstExpression: IAstNode
                {
                    IEnumerable<IAstNode> Children { get; }
                    void Append(IAstNode node);
                    void Complete(ref IAstExpression current);
                    void Replace(IAstNode previous, IAstNode replacement);
                }
                private class AstNode: IAstNode
                {
                    public AstNode(IAstExpression? parent)
                    {
                        Parent = parent;
                    }

                    public IAstExpression? Parent { get; }
                }
                private class AstLiteral: AstNode
                {
                    public readonly NumberToken Token;
                    public double Value => Token.Value;

                    public AstLiteral(IAstExpression? parent, NumberToken token) : base(parent)
                    {
                        Token = token;
                    }
                }
                private abstract class AstExpression<TNode> : AstNode, IAstExpression where TNode : AstNode
                {
                    protected readonly List<TNode> children = new List<TNode>();
                    public virtual IReadOnlyList<TNode> Children => children;
                    IEnumerable<IAstNode> IAstExpression.Children => Children;

                    public AstExpression(IAstExpression? parent) : base(parent) { }

                    public virtual void Append(IAstNode node) => Append((TNode)node);
                    public virtual void Append(TNode node)
                    {
                        children.Add(node);
                    }

                    public void Replace(IAstNode previous, IAstNode replacement) => Replace((TNode)previous, (TNode)replacement);
                    public void Replace(TNode previous, TNode replacement)
                    {
                        if (children.Replace(previous, replacement) == 0)
                        {
                            throw new InvalidOperationException("Failed to replace");
                        }
                    }

                    public virtual void Complete(ref IAstExpression current) {
                        if(current != this)
                        {
                            throw new InvalidOperationException("Desynced");
                        }
                    }
                }
                private class AstCompositeArithmeticExpression : AstExpression<AstNode>
                {
                    public AstCompositeArithmeticExpression(IAstExpression? parent) : base(parent){}

                    public override void Complete(ref IAstExpression current)
                    {
                        base.Complete(ref current);
                        // Handle negative unary
                        List<AstNode> negated = new(Children.Count);
                        for (var i = 0; i < Children.Count; i++)
                        {
                            if (
                                Children.Count >= i + 2 &&
                                Children[i] is AstArithmeticExpression arithmeticExpression && arithmeticExpression.Token.Value == Operator.Subtract &&
                                (i == 0 || Children[i - 1] is AstArithmeticExpression)
                            )
                            {
                                if (Children[i + 1] is AstLiteral literalExpression)
                                {
                                    negated.Add(new AstLiteral(this, new NumberToken(-literalExpression.Token.Value)));
                                }
                                else
                                {
                                    negated.Add(arithmeticExpression);
                                    negated.Add(Children[i + 1]);
                                }
                                i++;
                            }
                            else
                            {
                                negated.Add(Children[i]);
                            }
                        }

                        // Verify alternated expressions / operators
                        var operators = new List<(AstArithmeticExpression op, int index)>();
                        for (var i = 0; i < negated.Count; i++)
                        {
                            var expectedOperator = i % 2 == 1;
                            if (!(!expectedOperator || negated[i] is AstArithmeticExpression))
                            {
                                throw new InvalidOperationException();
                            }
                            if (expectedOperator)
                            {
                                operators.Add(((AstArithmeticExpression)negated[i], i));
                            }
                        }

                        var orderedOperators = operators.OrderBy(pair => pair.op.Token.Value, new OperatorOrderComparer()).ToList();

                        var last = negated[0];
                        foreach (var (orderedOperator, index) in orderedOperators)
                        {
                            var npnOperator = new AstBinaryExpression(Parent, orderedOperator.Token, negated[index - 1], negated[index + 1]);
                            last = npnOperator;
                            negated = negated
                                .Take(index - 1)
                                .Append(npnOperator).Append(npnOperator).Append(npnOperator)
                                .Concat(negated.Skip(index + 2))
                                .ToList();
                        }

                        Parent.Replace(this, last);
                        current = Parent;
                    }
                }
                private class AstCallExpressionArg : AstCompositeArithmeticExpression
                {
                    public new AstCallExpression Parent { get => (AstCallExpression)base.Parent; }
                    public static AstCallExpressionArg Init(AstCallExpression parent, ref IAstExpression context)
                    {
                        var arg = new AstCallExpressionArg(parent);
                        context = arg;
                        return arg;
                    }
                    private AstCallExpressionArg(AstCallExpression? parent) : base(parent) { }

                    public override void Complete(ref IAstExpression current)
                    {
                        // Functions are always initialized with a placeholder arg.
                        // Normalize arithmetic operation only for functions with args.
                        if(Children.Count != 0 || Parent.Children.Count != 1)
                        {
                            base.Complete(ref current);
                        }
                        current = Parent;
                        current.Complete(ref current);
                    }
                    public void CompleteForNewArg(ref IAstExpression current)
                    {
                        base.Complete(ref current);
                    }
                }
                private class AstCallExpression : AstExpression<AstNode>
                {
                    public readonly NameToken Token;
                    public string Name => Token.Value;

                    public static void Init(ref IAstExpression context, NameToken name)
                    {
                        var callExpression = new AstCallExpression(context, name);
                        context.Append(callExpression);
                        context = callExpression;
                        var arg = AstCallExpressionArg.Init(callExpression, ref context);
                        callExpression.Append(arg);
                    }

                    private AstCallExpression(IAstExpression? parent, NameToken token): base(parent)
                    {
                        Token = token;
                    }
                    public override void Complete(ref IAstExpression current)
                    {
                        base.Complete(ref current);
                        // Remove empty arg
                        if(Children.Count == 1 && Children[0] is AstCallExpressionArg arg && arg.Children.Count == 0)
                        {
                            children.Clear();
                        }
                        current = Parent;
                    }
                    public void NewArg(ref IAstExpression currentGroup)
                    {
                        if(currentGroup is not AstCallExpressionArg arg) {
                            throw new InvalidOperationException();
                        }
                        arg.CompleteForNewArg(ref currentGroup);
                        var nextArgGroup = AstCallExpressionArg.Init(this, ref currentGroup);
                        children.Add(nextArgGroup);
                        currentGroup = nextArgGroup;
                    }
                }
                private class AstArithmeticExpression : AstExpression<AstNode>
                {
                    public readonly OperatorToken Token;
                    public Operator Operator => Token.Value;

                    public AstArithmeticExpression(IAstExpression? parent, OperatorToken token, List<AstNode>? children = null): base(parent)
                    {
                        Token = token;
                        if(children != null)
                        {
                            this.children.AddRange(children);
                        }
                    }
                }
                private class AstUnaryExpression : AstArithmeticExpression
                {
                    public AstNode Child => Children.Single();

                    public AstUnaryExpression(IAstExpression? parent, OperatorToken @operator, AstNode child) : base(parent, @operator, new() { child }) { }
                }
                private class AstBinaryExpression : AstArithmeticExpression
                {
                    public AstNode Left => Children[0];
                    public AstNode Right => Children[1];

                    public AstBinaryExpression(IAstExpression? parent, OperatorToken @operator, AstNode left, AstNode right) : base(parent, @operator, new() { left, right }) { }
                }
                private static AstNode ToAst(IEnumerable<IToken> tokens)
                {
                    var rootExpression = new AstCompositeArithmeticExpression(null);
                    IAstExpression currentExpression = new AstCompositeArithmeticExpression(rootExpression);
                    rootExpression.Append(currentExpression);
                    var enumerator = tokens.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var currentToken = enumerator.Current;
                        switch (currentToken)
                        {
                            case NumberToken numberToken:
                                currentExpression.Append(new AstLiteral(currentExpression, numberToken));
                                break;
                            case OperatorToken operatorToken:
                                if (operatorToken.Value == Operator.OpenGroup)
                                {
                                    var newGroup = new AstCompositeArithmeticExpression(currentExpression);
                                    currentExpression.Append(newGroup);
                                    currentExpression = newGroup;
                                }
                                else if (operatorToken.Value == Operator.CloseGroup)
                                {
                                    currentExpression.Complete(ref currentExpression);
                                }
                                else if (operatorToken.Value == Operator.ArgSep)
                                {
                                    if (currentExpression is AstCallExpressionArg callArg)
                                    {
                                        callArg.Parent.NewArg(ref currentExpression);
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("Found invalid `,` in non function call");
                                    }
                                }
                                else
                                {
                                    currentExpression.Append(new AstArithmeticExpression(currentExpression, operatorToken));
                                }
                                break;
                            case NameToken nameToken:
                                if (enumerator.MoveNext())
                                {
                                    var next = enumerator.Current;
                                    if(next is OperatorToken operatorToken && operatorToken.Value == Operator.OpenGroup)
                                    {
                                        AstCallExpression.Init(ref currentExpression, nameToken);
                                    } else
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    currentExpression.Complete(ref currentExpression);
                    if (currentExpression != rootExpression)
                    {
                        throw new InvalidOperationException("Desynced AST");
                    }

                    return rootExpression;
                }

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
            #endregion
        }
    }
}
