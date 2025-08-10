using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness
    {
        internal partial class Parser
        {
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

            internal interface IAstNode
            {
                IAstExpression? Parent { get; }
            }
            internal interface IAstExpression : IAstNode
            {
                IEnumerable<IAstNode> Children { get; }
                void Append(IAstNode node);
                void Complete(ref IAstExpression astCursor);
                void Replace(IAstNode previous, IAstNode replacement);
            }
            internal class AstNode : IAstNode
            {
                public AstNode(IAstExpression? parent)
                {
                    Parent = parent;
                }

                public IAstExpression? Parent { get; }
            }
            private class AstLiteral : AstNode
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

                public virtual void Complete(ref IAstExpression astCursor)
                {
                    if (astCursor != this)
                    {
                        throw new InvalidOperationException("Desynced");
                    }
                }
            }
            private class AstCompositeArithmeticExpression : AstExpression<AstNode>
            {
                public AstCompositeArithmeticExpression(IAstExpression? parent) : base(parent) { }

                public override void Complete(ref IAstExpression astCursor)
                {
                    base.Complete(ref astCursor);
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
                    astCursor = Parent;
                }
            }
            private class AstCallExpressionArg : AstCompositeArithmeticExpression
            {
                public new AstCallExpression Parent { get => (AstCallExpression)base.Parent; }
                public AstCallExpressionArg(AstCallExpression? parent) : base(parent) { }

                /// <summary>
                /// Terminate the complete function call. This generates <see cref="AstArithmeticExpression" /> and terminate the function node.
                /// </summary>
                public override void Complete(ref IAstExpression astCursor)
                {
                    // Functions are always initialized with a placeholder arg.
                    // Normalize arithmetic operation only for functions with args.
                    if (Children.Count != 0 || Parent.Children.Count != 1)
                    {
                        base.Complete(ref astCursor);
                    }
                    astCursor = Parent;
                    astCursor.Complete(ref astCursor);
                }
                /// <summary>
                /// Only build <see cref="AstArithmeticExpression" />, but stay in function node.
                /// </summary>
                public void CompleteForNewArg(ref IAstExpression astCursor)
                {
                    base.Complete(ref astCursor);
                }
            }
            private class AstCallExpression : AstExpression<AstNode>
            {
                public readonly NameToken Token;
                public string Name => Token.Value;

                public AstCallExpression(IAstExpression? parent, NameToken token) : base(parent)
                {
                    Token = token;
                }
                public override void Complete(ref IAstExpression astCursor)
                {
                    base.Complete(ref astCursor);
                    // Remove empty placeholder arg for nullary functions
                    if (Children.Count == 1 && Children[0] is AstCallExpressionArg arg && arg.Children.Count == 0)
                    {
                        children.Clear();
                    }
                    astCursor = Parent;
                }
                public void NewArg(ref IAstExpression astCursor)
                {
                    if (astCursor is not AstCallExpressionArg arg)
                    {
                        throw new InvalidOperationException();
                    }
                    arg.CompleteForNewArg(ref astCursor);
                    var nextArgGroup = new AstCallExpressionArg(this);
                    children.Add(nextArgGroup);
                    astCursor = nextArgGroup;
                }
            }
            private class AstArithmeticExpression : AstExpression<AstNode>
            {
                public readonly OperatorToken Token;
                public Operator Operator => Token.Value;

                public AstArithmeticExpression(IAstExpression? parent, OperatorToken token, List<AstNode>? children = null) : base(parent)
                {
                    Token = token;
                    if (children != null)
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
            private class AstBindingExpression : AstNode
            {
                public readonly NameToken Token;
                public string Name => Token.Value;

                public AstBindingExpression(IAstExpression? parent, NameToken token): base(parent)
                {
                    Token = token;
                }
            }

            internal static AstNode ToAst(IEnumerable<IToken> tokens)
            {
                var rootExpression = new AstCompositeArithmeticExpression(null);
                IAstExpression astCursor = new AstCompositeArithmeticExpression(rootExpression);
                rootExpression.Append(astCursor);
                var allTokens = tokens.ToArray();
                for(var i = 0; i < allTokens.Length; i++)
                {
                    var currentToken = allTokens[i];
                    switch (currentToken)
                    {
                        case NumberToken numberToken:
                            astCursor.Append(new AstLiteral(astCursor, numberToken));
                            break;
                        case OperatorToken operatorToken:
                            if (operatorToken.Value == Operator.OpenGroup)
                            {
                                var newGroup = new AstCompositeArithmeticExpression(astCursor);
                                astCursor.Append(newGroup);
                                astCursor = newGroup;
                            }
                            else if (operatorToken.Value == Operator.CloseGroup)
                            {
                                astCursor.Complete(ref astCursor);
                            }
                            else if (operatorToken.Value == Operator.ArgSep)
                            {
                                if (astCursor is AstCallExpressionArg callArg)
                                {
                                    callArg.Parent.NewArg(ref astCursor);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Found invalid `,` in non function call");
                                }
                            }
                            else
                            {
                                astCursor.Append(new AstArithmeticExpression(astCursor, operatorToken));
                            }
                            break;
                        case NameToken nameToken:
                            {
                                // Name are either function calls `foo(1+2)` or variable binding
                                if (i + 1 < allTokens.Length && allTokens[i + 1] is OperatorToken operatorToken && operatorToken.Value == Operator.OpenGroup)
                                {
                                    i++;
                                    var callExpression = new AstCallExpression(astCursor, nameToken);
                                    astCursor.Append(callExpression);
                                    astCursor = callExpression;
                                    var arg = new AstCallExpressionArg(callExpression);
                                    callExpression.Append(arg);
                                    astCursor = arg;
                                }
                                else
                                {
                                    var newNode = new AstBindingExpression(astCursor, nameToken);
                                    astCursor.Append(newNode);
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                astCursor.Complete(ref astCursor);
                if (astCursor != rootExpression)
                {
                    throw new InvalidOperationException("Desynced AST");
                }

                return rootExpression;
            }
        }
    }
}
