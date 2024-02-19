using System;
using System.Collections.Generic;
using System.IO;
using uDataBinder.Binder;
using UnityEngine;

namespace uDataBinder
{
    delegate (LexerToken, object) ConditionExpression(DataBinder dataBinder);
    enum LexerToken
    {
        OP_OBJECT,
        OP_SYMBOL,
        OP_NOT,
        OP_AND,
        OP_OR,
        OP_EQUAL,
        OP_NOT_EQUAL,
        OP_LESS,
        OP_LESS_EQUAL,
        OP_GREATER,
        OP_GREATER_EQUAL,
        OP_LEFT_PAREN,
        OP_RIGHT_PAREN,
        OP_EOF,
    }
    struct ConditionParser
    {
        public static bool EvalBooleanValue(ConditionExpression func, DataBinder dataBinder)
        {
            var value = func(dataBinder);
            switch (value.Item1)
            {
                case LexerToken.OP_OBJECT:
                    return (bool)value.Item2;
                case LexerToken.OP_SYMBOL:
                    string condition = (string)value.Item2;
                    DataBinding.Register(condition, dataBinder);
                    return DataBinding.GetValue<bool>(condition, dataBinder?.gameObject);
                default:
                    Debug.LogError($"invalid token {value.Item1}");
                    return false;
            }
        }
        public static object EvalValue(ConditionExpression func, DataBinder dataBinder)
        {
            var value = func(dataBinder);
            switch (value.Item1)
            {
                case LexerToken.OP_OBJECT:
                    return value.Item2;
                case LexerToken.OP_SYMBOL:
                    string condition = (string)value.Item2;
                    DataBinding.Register(condition, dataBinder);
                    return DataBinding.GetValue(condition, dataBinder?.gameObject);
                default:
                    Debug.LogError($"invalid token {value.Item1}");
                    return false;
            }
        }
        private readonly StringReader reader;
        private (LexerToken, object)? lex_value;
        private readonly char? Peek()
        {
            var ret = reader.Peek();
            return ret == -1 ? null : (char?)ret;
        }
        private readonly char? Read()
        {
            var ret = reader.Read();
            return ret == -1 ? null : (char?)ret;
        }
        private readonly char? Read(params char[] chars)
        {
            var peek = reader.Peek();
            if (peek == -1)
            {
                return null;
            }
            var c = (char)peek;
            foreach (var ch in chars)
            {
                if (ch == c)
                {
                    reader.Read();
                    return c;
                }
            }
            return null;
        }

        public ConditionParser(string condition)
        {
            reader = new StringReader(condition);
            lex_value = null;
        }
        private readonly void SkipSpace()
        {
            while (true)
            {
                var peek = Peek();
                if (peek == null || !Char.IsWhiteSpace(peek.Value))
                {
                    break;
                }
                reader.Read();
            }
        }
        private (LexerToken, object) PeekToken()
        {
            if (lex_value != null)
            {
                return lex_value.Value;
            }
            SkipSpace();
            var lex = Peek();
            switch (lex)
            {
                case null:
                    return (LexerToken.OP_EOF, null);
                case '&':
                    Read();
                    if (Read('&') != null)
                    {
                        lex_value = (LexerToken.OP_AND, null);
                        return lex_value.Value;
                    }
                    break;
                case '|':
                    Read();
                    if (Read('|') != null)
                    {
                        lex_value = (LexerToken.OP_OR, null);
                        return lex_value.Value;
                    }
                    break;
                case '<':
                    Read();
                    lex_value = (Read('=') != null ? LexerToken.OP_LESS_EQUAL : LexerToken.OP_LESS, null);
                    return lex_value.Value;
                case '>':
                    Read();
                    lex_value = (Read('=') != null ? LexerToken.OP_GREATER_EQUAL : LexerToken.OP_GREATER, null);
                    return lex_value.Value;
                case '!':
                    Read();
                    lex_value = (Read('=') != null ? LexerToken.OP_NOT_EQUAL : LexerToken.OP_NOT, null);
                    return lex_value.Value;
                case '=':
                    Read();
                    if (Read('=') != null)
                    {
                        lex_value = (LexerToken.OP_EQUAL, null);
                        return lex_value.Value;
                    }
                    break;
                case '(':
                    Read();
                    lex_value = (LexerToken.OP_LEFT_PAREN, null);
                    return lex_value.Value;
                case ')':
                    Read();
                    lex_value = (LexerToken.OP_RIGHT_PAREN, null);
                    return lex_value.Value;
                default:
                    if (char.IsNumber(lex.Value) || lex.Value == '-')
                    {
                        var sw = new StringWriter();
                        Read();
                        sw.Write(lex.Value);
                        lex = Peek();
                        while (lex != null && (char.IsNumber(lex.Value) || lex.Value == '.'))
                        {
                            Read();
                            sw.Write(lex.Value);
                            lex = Peek();
                        }
                        var condition = sw.ToString();
                        return (LexerToken.OP_OBJECT,
                            condition.Contains(".") ?
                            double.Parse(condition) :
                            int.Parse(condition));
                    }
                    else if (lex.Value == '"')
                    {
                        var sw = new StringWriter();
                        Read();
                        lex = Peek();
                        while (lex != null && lex.Value != '"')
                        {
                            Read();
                            sw.Write(lex.Value);
                            lex = Peek();
                        }
                        Read();
                        return (LexerToken.OP_OBJECT, sw.ToString());
                    }
                    else
                    {
                        var sw = new StringWriter();
                        while (lex != null)
                        {
                            if ("&|<>!=()".Contains(lex.Value.ToString()) || char.IsWhiteSpace(lex.Value))
                            {
                                break;
                            }
                            Read();
                            sw.Write(lex.Value);
                            SkipSpace();
                            lex = Peek();
                        }
                        var symbol = sw.ToString();
                        if (symbol == "null")
                        {
                            return (LexerToken.OP_OBJECT, null);
                        }
                        if (symbol == "true")
                        {
                            return (LexerToken.OP_OBJECT, true);
                        }
                        if (symbol == "false")
                        {
                            return (LexerToken.OP_OBJECT, false);
                        }
                        if (symbol == "")
                        {
                            return (LexerToken.OP_OBJECT, false);
                        }
                        return (LexerToken.OP_SYMBOL, symbol);
                    }
            }
            return (LexerToken.OP_OBJECT, null);
        }
        private (LexerToken, object) ParseToken()
        {
            var result = PeekToken();
            lex_value = null;
            return result;
        }

        private LexerToken? ParseComparableToken()
        {
            var token = PeekToken();
            switch (token.Item1)
            {
                case LexerToken.OP_EQUAL:
                case LexerToken.OP_NOT_EQUAL:
                case LexerToken.OP_LESS:
                case LexerToken.OP_LESS_EQUAL:
                case LexerToken.OP_GREATER:
                case LexerToken.OP_GREATER_EQUAL:
                    ParseToken();
                    return token.Item1;
                default:
                    return null;
            }
        }

        private ConditionExpression ParseValue()
        {
            var token = PeekToken();
            if (token.Item1 == LexerToken.OP_NOT)
            {
                ParseToken();
                var result = ParseValue();
                return (dataBinder) => (LexerToken.OP_OBJECT, !EvalBooleanValue(result, dataBinder));
            }
            else if (token.Item1 == LexerToken.OP_LEFT_PAREN)
            {
                ParseToken();
                var result = ParseTop();
                var token2 = PeekToken();
                if (token2.Item1 != LexerToken.OP_RIGHT_PAREN)
                {
                    Debug.LogError($"invalid token {token2} #1");
                }
                else
                {
                    ParseToken();
                }
                return result;
            }
            else if (token.Item1 == LexerToken.OP_OBJECT || token.Item1 == LexerToken.OP_SYMBOL)
            {
                return (dataBinder) => token;
            }
            else
            {
                Debug.LogError($"invalid token {token} #2");
                return (dataBinder) => (LexerToken.OP_OBJECT, null);
            }
        }

        private ConditionExpression ParseCondition()
        {
            var left = ParseValue();
            var token = ParseComparableToken();
            if (token == null)
            {
                return left;
            }
            var right = ParseValue();
            return dataBinder =>
            {
                var leftValue = EvalValue(left, dataBinder);
                var rightValue = EvalValue(right, dataBinder);
                if (leftValue != null && rightValue != null && leftValue.GetType() != rightValue.GetType())
                {
                    try
                    {
                        rightValue = Convert.ChangeType(rightValue, leftValue.GetType());
                    }
                    catch (Exception)
                    {
                        Debug.LogError("Type mismatch in conditional expression.");
                        return (LexerToken.OP_OBJECT, false);
                    }
                }
                bool resultValue = false;
                if (leftValue is IComparable)
                {
                    var comp = ((IComparable)leftValue).CompareTo(rightValue);
                    switch (token)
                    {
                        case LexerToken.OP_EQUAL:
                            resultValue = comp == 0;
                            break;
                        case LexerToken.OP_NOT_EQUAL:
                            resultValue = comp != 0;
                            break;
                        case LexerToken.OP_LESS:
                            resultValue = comp < 0;
                            break;
                        case LexerToken.OP_LESS_EQUAL:
                            resultValue = comp <= 0;
                            break;
                        case LexerToken.OP_GREATER:
                            resultValue = comp > 0;
                            break;
                        case LexerToken.OP_GREATER_EQUAL:
                            resultValue = comp >= 0;
                            break;
                    }
                }
                else
                {
                    switch (token)
                    {
                        case LexerToken.OP_EQUAL:
                            resultValue = leftValue == rightValue;
                            break;
                        case LexerToken.OP_NOT_EQUAL:
                            resultValue = leftValue != rightValue;
                            break;
                        default:
                            Debug.LogError("Type mismatch in conditional expression.");
                            resultValue = false;
                            break;
                    }
                }
                return (LexerToken.OP_OBJECT, resultValue);
            };
        }
        private ConditionExpression ParseAnd()
        {
            var list = new List<ConditionExpression>();
            var top = ParseCondition();
            while (PeekToken().Item1 == LexerToken.OP_AND)
            {
                ParseToken();
                list.Add(ParseCondition());
            }
            if (list.Count == 0)
            {
                return top;
            }
            else
            {
                return (dataBinder) =>
                {
                    var topValue = EvalBooleanValue(top, dataBinder);
                    if (!topValue)
                    {
                        return (LexerToken.OP_OBJECT, false);
                    }
                    foreach (var expr in list)
                    {
                        if (!EvalBooleanValue(expr, dataBinder))
                        {
                            return (LexerToken.OP_OBJECT, false);
                        }
                    }
                    return (LexerToken.OP_OBJECT, true);
                };
            }
        }
        private ConditionExpression ParseOr()
        {
            var list = new List<ConditionExpression>();
            var top = ParseAnd();
            while (PeekToken().Item1 == LexerToken.OP_OR)
            {
                ParseToken();
                list.Add(ParseAnd());
            }
            if (list.Count == 0)
            {
                return top;
            }
            else
            {
                return (dataBinder) =>
                {
                    var topValue = EvalBooleanValue(top, dataBinder);
                    if (topValue)
                    {
                        return (LexerToken.OP_OBJECT, true);
                    }
                    foreach (var expr in list)
                    {
                        if (EvalBooleanValue(expr, dataBinder))
                        {
                            return (LexerToken.OP_OBJECT, true);
                        }
                    }
                    return (LexerToken.OP_OBJECT, false);
                };
            }
        }
        private ConditionExpression ParseTop()
        {
            return ParseOr();
        }
        public ConditionExpression Parse()
        {
            return ParseTop();
        }
    }

    public static class ConditionBinding
    {
        private static readonly Dictionary<string, ConditionExpression> cacheParser = new();
        public static bool Parse(string condition, DataBinder dataBinder = null)
        {
            GameObject baseObject = null;
            if (dataBinder != null)
            {
                baseObject = dataBinder.gameObject;
            }
            if (dataBinder != null)
            {
                DataBinding.Register(condition, dataBinder, baseObject);
            }

            if (cacheParser.TryGetValue(condition, out var value))
            {
                return ConditionParser.EvalBooleanValue(value, dataBinder);
            }
            else
            {
                value = new ConditionParser(condition).Parse();
                cacheParser.Add(condition, value);
                return ConditionParser.EvalBooleanValue(value, dataBinder);
            }
        }
    }
}