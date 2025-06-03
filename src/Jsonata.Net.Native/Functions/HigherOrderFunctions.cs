using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Eval;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Provides higher-order functions that operate on arrays and objects using callback functions for JSONata expressions.
/// </summary>
public static class HigherOrderFunctions
{
    private static bool FilterAcceptsElement(FunctionToken function, JToken element, int index, JArray array)
    {
        int filterArgsCount = function.RequiredArgsCount;
        List<JToken> args = new List<JToken>();
        if (filterArgsCount >= 1)
        {
            args.Add(element);
        };
        if (filterArgsCount >= 2)
        {
            args.Add(new JValue(index));
        };
        if (filterArgsCount >= 3)
        {
            args.Add(array);
        };
        JToken res = EvalProcessor.InvokeFunction(
            function: function,
            args: args,
            context: null,
            env: null! //TODO: pass some real environment?
        );
        bool result = Eval.Helpers.Booleanize(res);
        return result;
    }

    /// <summary>
    /// Returns an array containing the results of applying the function parameter to each value in the array parameter.
    /// The function that is supplied as the second parameter must have the following signature:
    /// function(value [, index [, array]])
    /// Each value in the input array is passed in as the first parameter in the supplied function.
    /// The index (position) of that value in the input array is passed in as the second parameter, if specified.
    /// The whole input array is passed in as the third parameter, if specified.
    /// </summary>
    /// <param name="array">The array to map over</param>
    /// <param name="function">The function to apply to each element</param>
    /// <returns>Array of function results</returns>
    [FunctionName("map")]
    public static JToken Map([PropagateUndefined][PackSingleValueToSequence] JArray array, FunctionToken function)
    {
        int funcArgsCount = function.RequiredArgsCount;

        Sequence result = new Sequence();

        int index = 0;
        foreach (JToken element in array.ChildrenTokens)
        {
            List<JToken> args = new List<JToken>();
            if (funcArgsCount >= 1)
            {
                args.Add(element);
            };
            if (funcArgsCount >= 2)
            {
                args.Add(new JValue(index));
            };
            if (funcArgsCount >= 3)
            {
                args.Add(array);
            };
            JToken res = EvalProcessor.InvokeFunction(
                function: function,
                args: args,
                context: null,
                env: null! //TODO: pass some real environment?
            );
            if (res.Type != JTokenType.Undefined)
            {
                result.Add(res);
            };
            ++index;
        }
        return result;
    }

    /// <summary>
    /// Returns an array containing only the values in the array parameter that satisfy the function predicate (i.e. function returns Boolean true when passed the value).
    /// The function that is supplied as the second parameter must have the following signature:
    /// function(value [, index [, array]])
    /// Each value in the input array is passed in as the first parameter in the supplied function.
    /// The index (position) of that value in the input array is passed in as the second parameter, if specified.
    /// The whole input array is passed in as the third parameter, if specified.
    /// </summary>
    /// <param name="array">The array to filter</param>
    /// <param name="function">The predicate function</param>
    /// <returns>Filtered array</returns>
    [FunctionName("filter")]
    public static JToken Filter([PropagateUndefined][PackSingleValueToSequence] JArray array, FunctionToken function)
    {
        Sequence result = new Sequence();
        int index = 0;
        foreach (JToken element in array.ChildrenTokens)
        {
            if (FilterAcceptsElement(function, element, index, array))
            {
                result.Add(element);
            }
            ++index;
        }
        //return result.Simplify();
        return result;
    }

    /// <summary>
    /// Returns the one and only one value in the array parameter that satisfy the function predicate (i.e. function returns Boolean true when passed the value).
    /// Throws an exception if the number of matching values is not exactly one.
    /// The function that is supplied as the second parameter must have the following signature:
    /// function(value [, index [, array]])
    /// Each value in the input array is passed in as the first parameter in the supplied function.
    /// The index (position) of that value in the input array is passed in as the second parameter, if specified.
    /// The whole input array is passed in as the third parameter, if specified.
    /// </summary>
    /// <param name="array">The array to search</param>
    /// <param name="function">Optional predicate function</param>
    /// <returns>The single matching element</returns>
    [FunctionName("single")]
    public static JToken Single([PropagateUndefined][PackSingleValueToSequence] JArray array, [OptionalArgument(null)] FunctionToken? function)
    {
        JToken? result = null;
        int index = 0;
        foreach (JToken element in array.ChildrenTokens)
        {
            bool filterPassed = function != null ?
                FilterAcceptsElement(function, element, index, array)
                : true;
            if (filterPassed)
            {
                if (result != null)
                {
                    throw new JsonataException("D3138", "The $single() function expected exactly 1 matching result.  Instead it matched more.");
                }
                else
                {
                    result = element;
                }
            }
            ++index;
        }

        if (result == null)
        {
            throw new JsonataException("D3139", "The $single() function expected exactly 1 matching result.  Instead it matched 0.");
        }
        return result;
    }

    /// <summary>
    /// Returns an aggregated value derived from applying the function parameter successively to each value in array in combination with the result of the previous application of the function.
    /// The function must accept at least two arguments, and behaves like an infix operator between each value within the array.
    /// The signature of this supplied function must be of the form:
    /// myfunc($accumulator, $value[, $index[, $array]])
    /// If the optional init parameter is supplied, then that value is used as the initial value in the aggregation (fold) process.
    /// If not supplied, the initial value is the first value in the array parameter.
    /// </summary>
    /// <param name="array">The array to reduce</param>
    /// <param name="function">The reducer function</param>
    /// <param name="init">Optional initial value</param>
    /// <returns>The reduced value</returns>
    [FunctionName("reduce")]
    public static JToken Reduce([PropagateUndefined][PackSingleValueToSequence] JArray array, FunctionToken function, [OptionalArgument(null)] JToken? init)
    {
        JToken accumulator;
        IEnumerable<JToken> elements;
        int index;
        if (init == null || init.Type == JTokenType.Undefined)
        {
            if (array.Count == 0)
            {
                return EvalProcessor.UNDEFINED;
            };
            accumulator = array.ChildrenTokens[0];
            elements = array.ChildrenTokens.Skip(1);
            index = 1;
        }
        else
        {
            accumulator = init;
            elements = array.ChildrenTokens;
            index = 0;
        };

        int funcArgsCount = function.RequiredArgsCount;
        if (funcArgsCount < 2)
        {
            throw new JsonataException("D3050", "The second argument of reduce function must be a function with at least two arguments");
        }

        foreach (JToken element in elements)
        {
            List<JToken> args = new List<JToken>(funcArgsCount);
            args.Add(accumulator);
            args.Add(element);
            if (funcArgsCount >= 3)
            {
                args.Add(new JValue(index));
            };
            if (funcArgsCount >= 4)
            {
                args.Add(array);
            };
            accumulator = EvalProcessor.InvokeFunction(
                function: function,
                args: args,
                context: null,
                env: null! //TODO: pass some real environment?
            );
            ++index;
        }
        return accumulator;
    }

    /// <summary>
    /// Returns an object that contains only the key/value pairs from the object parameter that satisfy the predicate function passed in as the second parameter.
    /// If object is not specified, then the context value is used as the value of object.
    /// It is an error if object is not an object.
    /// The function that is supplied as the second parameter must have the following signature:
    /// function(value [, key [, object]])
    /// Each value in the input object is passed in as the first parameter in the supplied function.
    /// The key (property name) of that value in the input object is passed in as the second parameter, if specified.
    /// The whole input object is passed in as the third parameter, if specified.
    /// </summary>
    /// <param name="obj">The object to filter</param>
    /// <param name="function">The predicate function</param>
    /// <returns>Filtered object</returns>
    [FunctionName("sift")]
    public static JToken Sift([AllowContextAsValue][PropagateUndefined] JObject obj, FunctionToken function)
    {
        JObject result = new JObject();
        foreach (KeyValuePair<string, JToken> property in obj.Properties)
        {
            if (filterAcceptsElement(property.Value, property.Key, obj))
            {
                result.Add(property.Key, property.Value);
            }
        }
        if (result.Count == 0)
        {
            return EvalProcessor.UNDEFINED;
        }
        return result;

        bool filterAcceptsElement(JToken value, string key, JObject obj)
        {
            List<JToken> args = new List<JToken>();
            int filterArgsCount = function.RequiredArgsCount;
            if (filterArgsCount >= 1)
            {
                args.Add(value);
            };
            if (filterArgsCount >= 2)
            {
                args.Add(new JValue(key));
            };
            if (filterArgsCount >= 3)
            {
                args.Add(obj);
            };
            JToken res = EvalProcessor.InvokeFunction(
                function: function,
                args: args,
                context: null,
                env: null! //TODO: pass some real environment?
            );
            bool result = Eval.Helpers.Booleanize(res);
            return result;
        }
    }
}