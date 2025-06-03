using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;

namespace Jsonata.Net.Native.Eval;

/// <summary>
/// Evaluates operator nodes including arithmetic, comparison, boolean, negation, and string concatenation operations.
/// Handles type conversion, operator precedence, and validation for JSONata expression operators.
/// </summary>
internal sealed class OperatorEvaluator
{
    private readonly NodeEvaluator evaluateNode;

    internal OperatorEvaluator(NodeEvaluator evaluateNode)
    {
        this.evaluateNode = evaluateNode;
    }

    internal JToken EvalNegation(NegationNode negationNode, JToken input, EvaluationEnvironment env)
    {
        JToken rhs = evaluateNode(negationNode.rhs, input, env);
        switch (rhs.Type)
        {
            case JTokenType.Undefined:
                return JsonataEvaluator.UNDEFINED;
            case JTokenType.Integer:
                return new JValue(-(long)rhs);
            case JTokenType.Float:
                return new JValue(-(double)rhs);
            default:
                throw new JsonataException("D1002", $"Cannot negate a non-numeric value: {rhs}");
        }
    }

    internal JToken EvalNumericOperator(NumericOperatorNode numericOperatorNode, JToken input, EvaluationEnvironment env)
    {
        JToken lhs = evaluateNode(numericOperatorNode.lhs, input, env);
        JToken rhs = evaluateNode(numericOperatorNode.rhs, input, env);
        if (lhs.Type == JTokenType.Undefined || rhs.Type == JTokenType.Undefined)
        {
            return JsonataEvaluator.UNDEFINED;
        }
        else if (lhs.Type == JTokenType.Integer && rhs.Type == JTokenType.Integer)
        {
            if (numericOperatorNode.op == NumericOperatorNode.Operator.Divide)
            {
                //divide is still in double
                return EvalDoubleOperator((long)lhs, (long)rhs, numericOperatorNode.op);
            }
            else
            {
                return EvalIntOperator((long)lhs, (long)rhs, numericOperatorNode.op);
            }
        }
        else if (lhs.Type == JTokenType.Float && rhs.Type == JTokenType.Float)
        {
            return EvalDoubleOperator((double)lhs, (double)rhs, numericOperatorNode.op);
        }
        else if (lhs.Type == JTokenType.Float && rhs.Type == JTokenType.Integer)
        {
            return EvalDoubleOperator((double)lhs, (double)(long)rhs, numericOperatorNode.op);
        }
        else if (lhs.Type == JTokenType.Integer && rhs.Type == JTokenType.Float)
        {
            return EvalDoubleOperator((double)(long)lhs, (double)rhs, numericOperatorNode.op);
        }
        else if (lhs.Type != JTokenType.Float && lhs.Type != JTokenType.Integer)
        {
            throw new JsonataException("T2001", $"The left side of the {NumericOperatorNode.OperatorToString(numericOperatorNode.op)} operator must evaluate to a number");
        }
        else
        {
            throw new JsonataException("T2002", $"The right side of the {NumericOperatorNode.OperatorToString(numericOperatorNode.op)} operator must evaluate to a number");
        }
    }

    internal JToken EvalComparisonOperator(ComparisonOperatorNode comparisonOperatorNode, JToken input, EvaluationEnvironment env)
    {
        JToken lhs = evaluateNode(comparisonOperatorNode.lhs, input, env);
        JToken rhs = evaluateNode(comparisonOperatorNode.rhs, input, env);
        if (lhs.Type == JTokenType.Undefined || rhs.Type == JTokenType.Undefined)
        {
            switch (comparisonOperatorNode.op)
            {
                case ComparisonOperatorNode.Operator.Equal:
                case ComparisonOperatorNode.Operator.NotEqual:
                case ComparisonOperatorNode.Operator.In:
                    return new JValue(false);
                default:
                    if (lhs.Type != JTokenType.Undefined && !IsComparable(lhs))
                    {
                        throw new JsonataException("T2010", $"Argument '{lhs}' of comparison is not comparable");
                    }
                    else if (rhs.Type != JTokenType.Undefined && !IsComparable(rhs))
                    {
                        throw new JsonataException("T2010", $"Argument '{rhs}' of comparison is not comparable");
                    }
                    else
                    {
                        return JsonataEvaluator.UNDEFINED;
                    }
            }
        }
        ;

        if (lhs.Type == JTokenType.Integer && rhs.Type == JTokenType.Float)
        {
            lhs = new JValue((double)(int)lhs);
        }
        else if (rhs.Type == JTokenType.Integer && lhs.Type == JTokenType.Float)
        {
            rhs = new JValue((double)(int)rhs);
        }
        ;

        switch (comparisonOperatorNode.op)
        {
            case ComparisonOperatorNode.Operator.Equal:
                return new JValue(JToken.DeepEquals(lhs, rhs));
            case ComparisonOperatorNode.Operator.NotEqual:
                return new JValue(!JToken.DeepEquals(lhs, rhs));
            case ComparisonOperatorNode.Operator.In:
                {
                    if (rhs.Type == JTokenType.Array)
                    {
                        JArray rhsArray = (JArray)rhs;
                        foreach (JToken rhsSubtoken in rhsArray.ChildrenTokens)
                        {
                            if (JToken.DeepEquals(lhs, rhsSubtoken))
                            {
                                return new JValue(true);
                            }
                        }
                        return new JValue(false);
                    }
                    else
                    {
                        return new JValue(JToken.DeepEquals(lhs, rhs));
                    }
                }
            default:
                {
                    if (!IsComparable(lhs))
                    {
                        throw new JsonataException("T2010", $"Argument '{lhs}' of comparison is not comparable");
                    }
                    else if (!IsComparable(rhs))
                    {
                        throw new JsonataException("T2010", $"Argument '{rhs}' of comparison is not comparable");
                    }
                    else if (lhs.Type != rhs.Type)
                    {
                        throw new JsonataException("T2009", $"Arguments '{lhs}' and '{rhs}' of comparison are of different types");
                    }
                    ;

                    if (lhs.Type == JTokenType.String)
                    {
                        return CompareStrings(comparisonOperatorNode.op, (string)lhs!, (string)rhs!);
                    }
                    else if (lhs.Type == JTokenType.Integer)
                    {
                        return CompareInts(comparisonOperatorNode.op, (long)lhs, (long)rhs);
                    }
                    else if (lhs.Type == JTokenType.Float)
                    {
                        return CompareDoubles(comparisonOperatorNode.op, (double)lhs, (double)rhs);
                    }
                    else
                    {
                        throw new Exception("Should not happen");
                    }
                }
        }
    }

    internal JToken EvalBooleanOperator(BooleanOperatorNode booleanOperatorNode, JToken input, EvaluationEnvironment env)
    {
        bool lhs = evaluateNode(booleanOperatorNode.lhs, input, env).Booleanize(); //here undefined works as false? see boolize() in jsonata-js
                                                                       //short-cirquit the operators if possible:
        switch (booleanOperatorNode.op)
        {
            case BooleanOperatorNode.Operator.And:
                if (!lhs)
                {
                    return new JValue(false);
                }
                break;
            case BooleanOperatorNode.Operator.Or:
                if (lhs)
                {
                    return new JValue(true);
                }
                break;
        }
        ;


        bool rhs = evaluateNode(booleanOperatorNode.rhs, input, env).Booleanize();

        bool result = booleanOperatorNode.op switch
        {
            BooleanOperatorNode.Operator.And => lhs && rhs,
            BooleanOperatorNode.Operator.Or => lhs || rhs,
            _ => throw new ArgumentException($"Unexpected operator '{booleanOperatorNode.op}'")
        };
        return new JValue(result);
    }

    internal JToken EvalStringConcatenation(StringConcatenationNode stringConcatenationNode, JToken input, EvaluationEnvironment env)
    {
        string lstr = Stringify(evaluateNode(stringConcatenationNode.lhs, input, env));
        string rstr = Stringify(evaluateNode(stringConcatenationNode.rhs, input, env));
        return new JValue(lstr + rstr);
    }

    private JToken EvalIntOperator(long lhs, long rhs, NumericOperatorNode.Operator op)
    {
        long result = op switch
        {
            NumericOperatorNode.Operator.Add => lhs + rhs,
            NumericOperatorNode.Operator.Subtract => lhs - rhs,
            NumericOperatorNode.Operator.Multiply => lhs * rhs,
            NumericOperatorNode.Operator.Divide => lhs / rhs,
            NumericOperatorNode.Operator.Modulo => lhs % rhs,
            _ => throw new ArgumentException($"Unexpected operator '{op}'")
        };
        return new JValue(result);
    }

    private JToken EvalDoubleOperator(double lhs, double rhs, NumericOperatorNode.Operator op)
    {
        double result = op switch
        {
            NumericOperatorNode.Operator.Add => lhs + rhs,
            NumericOperatorNode.Operator.Subtract => lhs - rhs,
            NumericOperatorNode.Operator.Multiply => lhs * rhs,
            NumericOperatorNode.Operator.Divide => lhs / rhs,
            NumericOperatorNode.Operator.Modulo => lhs % rhs,
            _ => throw new ArgumentException($"Unexpected operator '{op}'")
        };
        long longResult = (long)result;
        if (longResult == result)
        {
            return new JValue(longResult);
        }
        else
        {
            return new JValue(result);
        }
    }

    private bool IsComparable(JToken token)
    {
        return token.Type == JTokenType.Integer
            || token.Type == JTokenType.Float
            || token.Type == JTokenType.String;
    }

    private JToken CompareDoubles(ComparisonOperatorNode.Operator op, double lhs, double rhs)
    {
        switch (op)
        {
            case ComparisonOperatorNode.Operator.Less:
                return new JValue(lhs < rhs);
            case ComparisonOperatorNode.Operator.LessEqual:
                return new JValue(lhs <= rhs);
            case ComparisonOperatorNode.Operator.Greater:
                return new JValue(lhs > rhs);
            case ComparisonOperatorNode.Operator.GreaterEqual:
                return new JValue(lhs >= rhs);
            default:
                throw new Exception("Should not happen");
        }
    }

    private JToken CompareInts(ComparisonOperatorNode.Operator op, long lhs, long rhs)
    {
        switch (op)
        {
            case ComparisonOperatorNode.Operator.Less:
                return new JValue(lhs < rhs);
            case ComparisonOperatorNode.Operator.LessEqual:
                return new JValue(lhs <= rhs);
            case ComparisonOperatorNode.Operator.Greater:
                return new JValue(lhs > rhs);
            case ComparisonOperatorNode.Operator.GreaterEqual:
                return new JValue(lhs >= rhs);
            default:
                throw new Exception("Should not happen");
        }
    }

    private JToken CompareStrings(ComparisonOperatorNode.Operator op, string lhs, string rhs)
    {
        switch (op)
        {
            case ComparisonOperatorNode.Operator.Less:
                return new JValue(String.CompareOrdinal(lhs, rhs) < 0);
            case ComparisonOperatorNode.Operator.LessEqual:
                return new JValue(String.CompareOrdinal(lhs, rhs) <= 0);
            case ComparisonOperatorNode.Operator.Greater:
                return new JValue(String.CompareOrdinal(lhs, rhs) > 0);
            case ComparisonOperatorNode.Operator.GreaterEqual:
                return new JValue(String.CompareOrdinal(lhs, rhs) >= 0);
            default:
                throw new Exception("Should not happen");
        }
    }

    private string Stringify(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Undefined:
                return "";
            case JTokenType.String:
                return (string)token!;
            case JTokenType.Array:
                {
                    JArray array = (JArray)token;
                    if (array is Sequence sequence && !sequence.keepSingletons && sequence.Count == 1)
                    {
                        return Stringify(array.ChildrenTokens[0]);
                    }
                    return array.ToFlatString();
                }
            default:
                return token.ToFlatString();
        }
    }
}