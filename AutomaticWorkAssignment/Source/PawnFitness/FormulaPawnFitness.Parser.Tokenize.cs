using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness
    {
        internal partial class Parser
        {
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
                                    throw new ParseException(new("AWA.FormulaEditor.Error.Number.MultiSep", formula.Substring(start, i - start)), new ArgumentException($"Invalid formula \"{formula}\" at index {i}", nameof(formula)));
                                }
                                hasDecimalSep = true;
                            }
                            i++;
                        }
                        if (IsValidNameChar(leading: false))
                        {
                            throw new ParseException(new("AWA.FormulaEditor.Error.Number.NoLetter", formula.Substring(start, i - start + 1)), new ArgumentException($"Invalid formula \"{formula}\" at index {i}", nameof(formula)));
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
                        throw new ParseException(new("AWA.FormulaEditor.Error.InvalidChar", formula[i]), new ArgumentException($"Invalid formula \"{formula}\" at index {i}", nameof(formula)));
                    }
                }
            }
        }
    }
}
