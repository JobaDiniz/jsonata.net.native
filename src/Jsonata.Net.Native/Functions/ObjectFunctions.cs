using Jsonata.Net.Native.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native;

/// <summary>
/// Provides object manipulation and inspection functions for JSONata expressions.
/// </summary>
public static class ObjectFunctions
{
    /// <summary>
    /// Returns an array containing the keys in the object.
    /// If the argument is an array of objects, then the array returned contains a de-duplicated list of all the keys in all of the objects.
    /// </summary>
    /// <param name="arg">The object or array of objects to get keys from</param>
    /// <returns>Array of keys or undefined if no keys found</returns>
    [FunctionName("keys")]
    public static JToken Keys([AllowContextAsValue][PropagateUndefined] JToken arg)
    {
        ICollection<string> keys;
        switch (arg.Type)
        {
            case JTokenType.Object:
                keys = ((JObject)arg).Keys;
                break;
            case JTokenType.Array:
                keys = ((JArray)arg).ChildrenTokens
                        .OfType<JObject>()
                        .SelectMany(o => o.Keys)
                        .Distinct()
                        .ToList();
                break;
            default:
                return JValue.Undefined;
        }
        if (keys.Count == 0)
        {
            return JValue.Undefined;
        }
        else if (keys.Count == 1)
        {
            return new JValue(keys.First()!);
        };
        JArray result = new JArray(keys.Count);
        foreach (string key in keys)
        {
            result.Add(new JValue(key));
        }
        return result;
    }

    /// <summary>
    /// Returns the value associated with key in object. If the first argument is an array of objects, then all of the objects in the array are searched, and the values associated with all occurrences of key are returned.
    /// </summary>
    /// <param name="arg">The object or array of objects to search</param>
    /// <param name="key">The key to look up</param>
    /// <returns>The value(s) associated with the key</returns>
    [FunctionName("lookup")]
    public static JToken Lookup(JToken arg, string key)
    {
        switch (arg.Type)
        {
            case JTokenType.Array:
                {
                    JArray result = new Sequence();
                    foreach (JToken child in ((JArray)arg).ChildrenTokens)
                    {
                        JToken res = Lookup(child, key);
                        if (res.Type == JTokenType.Array)
                        {
                            result.AddRange(((JArray)res).ChildrenTokens);
                        }
                        else if (res.Type != JTokenType.Undefined)
                        {
                            result.Add(res);
                        }
                    }
                    return result;
                };
            case JTokenType.Object:
                {
                    JObject obj = (JObject)arg;
                    if (obj.Properties.TryGetValue(key, out JToken? result))
                    {
                        return result;
                    }
                    else
                    {
                        return JValue.Undefined;
                    };
                };
            default:
                return JValue.Undefined;

        }
    }

    /// <summary>
    /// Splits an object containing key/value pairs into an array of objects, each of which has a single key/value pair from the input object.
    /// If the parameter is an array of objects, then the resultant array contains an object for every key/value pair in every object in the supplied array.
    /// </summary>
    /// <param name="arg">The object or array of objects to spread</param>
    /// <returns>Array of single key/value pair objects</returns>
    [FunctionName("spread")]
    public static JToken Spread([AllowContextAsValue][PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Object:
                {
                    JObject obj = (JObject)arg;
                    if (obj.Count == 0)
                    {
                        return JValue.Undefined;
                    }
                    JArray result = new JArray(obj.Properties.Count);
                    foreach (KeyValuePair<string, JToken> property in obj.Properties)
                    {
                        JObject subResult = new JObject();
                        subResult.Add(property.Key, property.Value);
                        result.Add(subResult);
                    };
                    return result;
                }
            case JTokenType.Array:
                {
                    JArray array = (JArray)arg;
                    if (array.Count == 0)
                    {
                        return JValue.Undefined;
                    }
                    JArray result = new JArray();
                    foreach (JToken element in array.ChildrenTokens)
                    {
                        JToken elementResult = Spread(element);
                        switch (elementResult.Type)
                        {
                            case JTokenType.Undefined:
                                break;
                            case JTokenType.Array:
                                result.AddRange(((JArray)elementResult).ChildrenTokens);
                                break;
                            default:
                                result.Add(elementResult);
                                break;
                        }
                    }
                    return result;
                }
            default:
                return arg;
        }
    }

    /// <summary>
    /// Merges an array of objects into a single object containing all the key/value pairs from each of the objects in the input array.
    /// If any of the input objects contain the same key, then the returned object will contain the value of the last one in the array. It is an error if the input array contains an item that is not an object.
    /// </summary>
    /// <param name="arg">Array of objects to merge</param>
    /// <returns>Merged object</returns>
    [FunctionName("merge")]
    public static JObject Merge([AllowContextAsValue][PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Object:
                return (JObject)arg;
            case JTokenType.Array:
                break;
            default:
                throw new JsonataException("T0412", $"Argument 1 of function \"{nameof(Merge)}\" must be an array of \"objects\"");
        };
        JArray array = (JArray)arg;
        JObject result = new JObject();
        foreach (JToken element in array.ChildrenTokens)
        {
            switch (element.Type)
            {
                case JTokenType.Undefined:
                    break;
                case JTokenType.Object:
                    {
                        JObject obj = (JObject)element;
                        foreach (KeyValuePair<string, JToken> property in obj.Properties)
                        {
                            result.Set(property.Key, property.Value);
                        };
                    }
                    break;
                default:
                    throw new JsonataException("T0412", $"Argument 1 of function \"{nameof(Merge)}\" must be an array of \"objects\"");
            }
        }
        return result;
    }

    /// <summary>
    /// Returns an array containing the values return by the function when applied to each key/value pair in the object.
    /// The function parameter will get invoked with two arguments:
    /// function(value, name)
    /// where the value parameter is the value of each name/value pair in the object and name is its name. The name parameter is optional.
    /// </summary>
    /// <param name="obj">The object to iterate over</param>
    /// <param name="function">The function to apply to each key/value pair</param>
    /// <returns>Array of function results</returns>
    [FunctionName("each")]
    public static JArray Each([AllowContextAsValue][PropagateUndefined] JObject obj, FunctionToken function)
    {
        int argsCount = function.RequiredArgsCount;
        Sequence result = new Sequence();
        foreach (KeyValuePair<string, JToken> prop in obj.Properties)
        {
            List<JToken> args = new List<JToken>();
            if (argsCount >= 1)
            {
                args.Add(prop.Value);
            };
            if (argsCount >= 2)
            {
                args.Add(new JValue(prop.Key));
            };
            JToken res = ((JToken)function).TryInvoke(
                args: args,
                context: null,
                env: null! //TODO: pass some real environment?
            );
            if (res.Type != JTokenType.Undefined)
            {
                result.Add(res);
            };
        }
        return result;
    }

    /// <summary>
    /// Deliberately throws an error with an optional message.
    /// </summary>
    /// <param name="message">Optional error message</param>
    /// <returns>This function never returns as it always throws an exception</returns>
    [FunctionName("error")]
    public static JToken Error([OptionalArgument(null)] string message)
    {
        throw new JsonataException("D3137", message ?? "$error() function evaluated");
    }

    /// <summary>
    /// If condition is true, the function returns undefined.
    /// If the condition is false, an exception is thrown with the message as the message of the exception.
    /// </summary>
    /// <param name="condition">The condition to test</param>
    /// <param name="message">Optional error message</param>
    /// <returns>Undefined if condition is true</returns>
    [FunctionName("assert")]
    public static JToken Assert(bool condition, [OptionalArgument(null)] string message)
    {
        if (!condition)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "$assert() statement failed";
            };
            throw new JsonataAssertFailedException(message);
        }
        else
        {
            return JValue.Undefined;
        }
    }

    /// <summary>
    /// Evaluates the type of value and returns one of the following strings:
    /// "null", "number", "string", "boolean", "array", "object", "function"
    /// Returns (non-string) undefined when value is undefined.
    /// </summary>
    /// <param name="value">The value to get the type of</param>
    /// <returns>String representation of the value's type</returns>
    [FunctionName("type")]
    public static string GetType([PropagateUndefined] JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.Null:
                return "null";
            case JTokenType.Integer:
            case JTokenType.Float:
                return "number";
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "boolean";
            case JTokenType.Array:
                return "array";
            case JTokenType.Object:
                return "object";
            case JTokenType.Function:
                return "function";
            default:
                throw new Exception("Unexpected JToken type " + value.Type);
        }
    }
}