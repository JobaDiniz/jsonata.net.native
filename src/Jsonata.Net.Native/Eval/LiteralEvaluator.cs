using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Syntax;

namespace Jsonata.Net.Native;

/// <summary>
/// Evaluates literal value nodes including strings, numbers, booleans, null, and regular expressions.
/// These nodes represent constant values in JSONata expressions and do not require recursive evaluation.
/// </summary>
internal sealed class LiteralEvaluator
{
    internal JToken EvalString(StringNode stringNode, JToken input, EvaluationEnvironment env)
    {
        return new JValue(stringNode.value);
    }

    internal JToken EvalNumber(NumberDoubleNode numberNode, JToken input, EvaluationEnvironment env)
    {
        return new JValue(numberNode.value);
    }

    internal JToken EvalNumber(NumberIntNode numberNode, JToken input, EvaluationEnvironment env)
    {
        return new JValue(numberNode.value);
    }

    internal JToken EvalBoolean(BooleanNode booleanNode, JToken input, EvaluationEnvironment env)
    {
        return new JValue(booleanNode.value);
    }

    internal JToken EvalNull(NullNode nullNode, JToken input, EvaluationEnvironment env)
    {
        return JValue.CreateNull();
    }

    internal JToken EvalRegex(RegexNode regexNode, JToken input, EvaluationEnvironment env)
    {
        return new FunctionTokenRegex(regexNode.regex);
    }
}