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

                private interface INpnNode { }
                private interface INpnOutput: INpnNode { }
                private interface INpnValue : INpnOutput { }
                private interface INpnGroup: INpnNode {

                    List<INpnNode> Children { get; }
                }
                private readonly struct NpnLiteral: INpnNode, INpnValue
                {
                    public readonly NumberToken Token;

                    public NpnLiteral(NumberToken token)
                    {
                        Token = token;
                    }
                }
                private class NpnGroup : INpnGroup, INpnValue
                {
                    public List<INpnNode> Children { get; private set; }

                    public NpnGroup(List<INpnNode>? children = null)
                    {
                        Children = children ?? new();
                    }

                    public INpnValue Sort()
                    {
                        // Handle negative unary
                        List<INpnNode> negated = new(Children.Count);
                        for (var i = 0; i < Children.Count; i++)
                        {
                            if (
                                Children.Count >= i + 2 &&
                                Children[i] is NpnOperator npnOperation && npnOperation.Token.Value == Operator.Subtract &&
                                (i == 0 || Children[i - 1] is NpnOperator)
                            )
                            {
                                if (Children[i + 1] is NpnLiteral literal)
                                {
                                    negated.Add(new NpnLiteral(new NumberToken(-literal.Token.Value)));
                                }
                                else if (Children[i + 1] is INpnValue value)
                                {
                                    negated.Add(new NpnUnaryOperation(npnOperation.Token, value));
                                } else
                                {
                                    negated.Add(npnOperation);
                                    negated.Add(Children[i + 1]);
                                }
                                i++;
                            } else
                            {
                                negated.Add(Children[i]);
                            }
                        }

                        // Verify alternated expressions / operators
                        var operators = new List<(NpnOperator op, int index)>();
                        for (var i = 0; i < negated.Count; i++)
                        {
                            var expectedOperator = i % 2 == 1;
                            if (!(expectedOperator ? negated[i] is NpnOperator : negated[i] is INpnValue))
                            {
                                throw new InvalidOperationException();
                            }
                            if(expectedOperator)
                            {
                                operators.Add(((NpnOperator)negated[i], i));
                            }
                        }

                        var orderedOperators = operators.OrderBy(pair => pair.op.Token.Value, new OperatorOrderComparer()).ToList();

                        var last = (INpnValue)negated[0];
                        foreach(var (orderedOperator, index) in orderedOperators)
                        {
                            var npnOperator = new NpnBinaryOperation(orderedOperator.Token, (INpnValue)negated[index - 1], (INpnValue)negated[index + 1]);
                            last = npnOperator;
                            negated = negated
                                .Take(index - 1)
                                .Append(npnOperator).Append(npnOperator).Append(npnOperator)
                                .Concat(negated.Skip(index + 2))
                                .ToList();
                        }
                        return last;
                    }
                }
                private readonly struct NpnCall: INpnGroup, INpnValue
                {
                    public readonly NameToken Name;
                    public List<INpnNode> Children { get; }

                    public NpnCall(NameToken name, List<INpnNode> children)
                    {
                        Name = name;
                        Children = children;
                    }
                }
                private class NpnOperator : INpnGroup
                {
                    public readonly OperatorToken Token;
                    public List<INpnNode> Children { get; }

                    public NpnOperator(OperatorToken token, List<INpnNode>? children = null)
                    {
                        Token = token;
                        Children = children ?? new();
                    }
                }
                private class NpnAnyOperator : NpnOperator, INpnOutput, INpnValue
                {
                    public NpnAnyOperator(NpnOperator @operator) : this(@operator.Token, @operator.Children) { }
                    public NpnAnyOperator(OperatorToken @operator, List<INpnNode>? children = null) : base(@operator, children) { }
                }
                private class NpnUnaryOperation : NpnAnyOperator
                {
                    public INpnValue Child => (INpnValue)Children.Single();

                    public NpnUnaryOperation(OperatorToken @operator, INpnValue child) : base(@operator, new() { child }) { }
                }
                private class NpnBinaryOperation : NpnAnyOperator
                {
                    public INpnValue Left => (INpnValue)Children[0];
                    public INpnValue Right => (INpnValue)Children[1];

                    public NpnBinaryOperation(OperatorToken @operator, INpnValue left, INpnValue right) : base(@operator, new() { left, right }) { }
                }
                private static INpnValue ToPolishNotation(IEnumerable<IToken> tokens)
                {
                    NpnGroup root = new(new());
                    NpnGroup currentGroup = root;
                    Stack<NpnGroup> parentGroup = new();
                    foreach (var currentToken in tokens)
                    {
                        switch (currentToken)
                        {
                            case NumberToken numberToken:
                                currentGroup.Children.Add(new NpnLiteral(numberToken));
                                break;
                            case OperatorToken operatorToken:
                                if (operatorToken.Value == Operator.OpenGroup)
                                {
                                    var newGroup = new NpnGroup();
                                    parentGroup.Push(currentGroup);
                                    currentGroup.Children.Add(newGroup);
                                    currentGroup = newGroup;
                                } else if (operatorToken.Value == Operator.CloseGroup) {
                                    var parent = parentGroup.Pop();
                                    if(parent.Children.Replace(currentGroup, currentGroup.Sort()) == 0)
                                    {
                                        throw new InvalidOperationException("Failed to replace");
                                    }
                                    currentGroup = parent;
                                } else
                                {
                                    currentGroup.Children.Add(new NpnOperator(operatorToken));
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    return currentGroup.Sort();
                }

                internal Context ParseTokens(IEnumerable<IToken> tokens)
                {                    
                    var npnGrammar = ToPolishNotation(tokens);
                    var queue = new List<INpnNode>(new[] { npnGrammar });

                    // Collect all, deep first
                    for(var i = 0; i < queue.Count; i++) {
                        var cursor = queue[i];
                        switch (cursor)
                        {
                            case NpnLiteral literal:
                                break;
                            
                            case INpnGroup group:
                                // Flatten empty groups (eg: `(1)`, `(-4.2)`, `(foo())`
                                if (group.Children.Count == 1 && group is not NpnOperator)
                                {
                                    queue.Remove(cursor);
                                    i--;
                                }
                                queue.AddRange(group.Children.Cast<INpnNode>());
                                break;

                            default:
                                throw new ArgumentException("Invalid token type", nameof(cursor));

                        }
                    }

                    queue.Reverse();
                    var npnExpr = new Dictionary<INpnNode, Expression>();
                    foreach (var cursor in queue)
                    {
                        switch (cursor)
                        {
                            case NpnLiteral literal:
                                root = Expression.Constant(literal.Token.Value, typeof(double));
                                break;

                            case NpnAnyOperator anyOperator:
                                switch (anyOperator.Token.Value)
                                {
                                    case Operator.Exp:
                                        root = Expression.Power(npnExpr[anyOperator.Children[0]], npnExpr[anyOperator.Children[1]]);
                                        break;
                                    case Operator.Root:
                                        root = null;
                                        if (anyOperator.Children[1] is NpnLiteral lit)
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
                                        root = Expression.Power(
                                            npnExpr[anyOperator.Children[0]],
                                            Expression.Divide(
                                                Expression.Constant((double)1, typeof(double)),
                                                npnExpr[anyOperator.Children[1]]
                                            )
                                        );
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

                            default:
                                throw new ArgumentException("Invalid token type", nameof(cursor));

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
