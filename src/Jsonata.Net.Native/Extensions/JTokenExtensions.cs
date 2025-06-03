using System;
using System.Collections.Generic;
using System.Linq;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Extensions;

/// <summary>
/// Extension methods for JToken and related JSON types.
/// </summary>
internal static class JTokenExtensions
{
    /// <summary>
    /// Determines whether the token represents an array of numbers.
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns>True if the token is an array containing only numbers</returns>
    public static bool IsArrayOfNumbers(this JToken token)
    {
        if (token.Type != JTokenType.Array)
        {
            return false;
        }
        foreach (JToken subtoken in ((JArray)token).ChildrenTokens)
        {
            if (subtoken.Type != JTokenType.Integer && subtoken.Type != JTokenType.Float)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether the token represents an array of strings.
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns>True if the token is an array containing only strings</returns>
    public static bool IsArrayOfStrings(this JToken token)
    {
        if (token.Type != JTokenType.Array)
        {
            return false;
        }
        foreach (JToken subtoken in ((JArray)token).ChildrenTokens)
        {
            if (subtoken.Type != JTokenType.String)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the double value from a numeric token.
    /// </summary>
    /// <param name="token">The numeric token</param>
    /// <returns>The double value</returns>
    /// <exception cref="Exception">Thrown if the token is not numeric</exception>
    public static double GetDoubleValue(this JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Float:
                return (double)token;
            case JTokenType.Integer:
                return (double)(long)token;
            default:
                throw new Exception("Not a number " + token.ToFlatString());
        }
    }

    /// <summary>
    /// Converts a JToken to its effective boolean value using JSONata rules.
    /// </summary>
    /// <param name="value">The token to convert</param>
    /// <returns>The boolean representation</returns>
    public static bool Booleanize(this JToken value)
    {
        // cast arg to its effective boolean value
        // boolean: unchanged
        // string: zero-length -> false; otherwise -> true
        // number: 0 -> false; otherwise -> true
        // null -> false
        // array: empty -> false; length > 1 -> true
        // object: empty -> false; non-empty -> true
        // function -> false

        switch (value.Type)
        {
            case JTokenType.Undefined:
                return false;
            case JTokenType.Array:
                {
                    JArray array = (JArray)value;
                    if (array.Count == 0)
                    {
                        return false;
                    }
                    else if (array.Count == 1)
                    {
                        return array.ChildrenTokens[0].Booleanize();
                    }
                    else
                    {
                        return array.ChildrenTokens.Any(c => c.Booleanize());
                    }
                };
            case JTokenType.String:
                return ((string)value!).Length > 0;
            case JTokenType.Integer:
                return ((long)value) != 0;
            case JTokenType.Float:
                return ((double)value) != 0.0;
            case JTokenType.Object:
                return ((JObject)value!).Count > 0;
            case JTokenType.Boolean:
                return (bool)value;
            case JTokenType.Function:
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Enumerates numeric values from an array, converting integers and floats to decimal.
    /// </summary>
    /// <param name="array">The array to enumerate</param>
    /// <param name="functionName">The name of the calling function (for error messages)</param>
    /// <param name="argIndex">The argument index (for error messages)</param>
    /// <returns>Enumerable of decimal values</returns>
    /// <exception cref="JsonataException">Thrown if non-numeric values are encountered</exception>
    public static IEnumerable<decimal> EnumerateNumericValues(this JArray array, string functionName, int argIndex)
    {
        foreach (JToken token in array.ChildrenTokens)
        {
            switch (token.Type)
            {
                case JTokenType.Integer:
                    yield return (long)token;
                    break;
                case JTokenType.Float:
                    yield return (decimal)token;
                    break;
                case JTokenType.Undefined:
                    //just skip
                    break;
                default:
                    throw new JsonataException("T0412", $"Argument {argIndex} of function {functionName} must be an array of numbers. Got {token.Type}");
            }
        }
    }
}