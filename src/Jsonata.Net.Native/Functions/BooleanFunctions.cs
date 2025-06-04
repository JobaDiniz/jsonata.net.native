using Jsonata.Net.Native.Json;
using System;

namespace Jsonata.Net.Native;

/// <summary>
/// Provides boolean operations and conversion functions for JSONata expressions.
/// </summary>
public static class BooleanFunctions
{
    /// <summary>
    /// Casts the argument to a Boolean using JSONata casting rules.
    /// Boolean: unchanged
    /// string: empty - false, non-empty - true
    /// number: 0 - false, non-zero - true
    /// null - false
    /// array: empty - false, contains a member that casts to true - true, all members cast to false - false
    /// object: empty - false, non-empty - true
    /// function - false
    /// </summary>
    /// <param name="arg">The value to convert to boolean</param>
    /// <returns>Boolean representation of the input value</returns>
    [FunctionName("boolean")]
    public static JToken ToBoolean([AllowContextAsValue] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Boolean:
                //Boolean: unchanged
                return arg;
            case JTokenType.String:
                //string: empty   false
                //string: non-empty   true
                return new JValue(((string)arg!) != "");
            case JTokenType.Integer:
                //number: 0	false
                //number: non-zero    true
                return new JValue(((long)arg) != 0);
            case JTokenType.Float:
                //number: 0	false
                //number: non-zero    true
                return new JValue(((double)arg) != 0);
            case JTokenType.Null:
                //null	false
                return new JValue(false);
            case JTokenType.Array:
                //array: empty	false
                //array: contains a member that casts to true true
                //array: all members cast to false    false
                foreach (JToken child in ((JArray)arg).ChildrenTokens)
                {
                    JToken childRes = BooleanFunctions.ToBoolean(child);
                    if (childRes.Type == JTokenType.Boolean && (bool)childRes)
                    {
                        return new JValue(true);
                    }
                }
                return new JValue(false);
            case JTokenType.Object:
                //object: empty   false
                //object: non-empty   true
                return new JValue(((JObject)arg).Count > 0);
            case JTokenType.Function:
                //function	false
                return new JValue(false);
            case JTokenType.Undefined:
                return arg;
            default:
                throw new ArgumentException("Unexpected arg type: " + arg.Type);
        }
    }

    /// <summary>
    /// Returns Boolean NOT on the argument. arg is first cast to a boolean.
    /// </summary>
    /// <param name="arg">The value to negate</param>
    /// <returns>Negated boolean value</returns>
    [FunctionName("not")]
    public static JToken Not([AllowContextAsValue] JToken arg)
    {
        arg = BooleanFunctions.ToBoolean(arg);
        if (arg.Type == JTokenType.Undefined)
        {
            return arg;
        }
        return new JValue(!(bool)arg);
    }

    /// <summary>
    /// Returns Boolean true if the arg expression evaluates to a value, or false if the expression does not match anything (e.g. a path to a non-existent field reference).
    /// </summary>
    /// <param name="arg">The value to test for existence</param>
    /// <returns>True if the value exists, false otherwise</returns>
    [FunctionName("exists")]
    public static bool Exists(JToken arg)
    {
        return arg.Type != JTokenType.Undefined;
    }
}