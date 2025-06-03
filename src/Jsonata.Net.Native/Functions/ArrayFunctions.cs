using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Provides array manipulation and aggregation functions for JSONata expressions.
/// </summary>
public static class ArrayFunctions
{
    /// <summary>
    /// Returns the number of items in the array parameter.
    /// If the array parameter is not an array, but rather a value of another JSON type, then the parameter is treated as a singleton array containing that value, and this function returns 1.
    /// If array is not specified, then the context value is used as the value of array.
    /// </summary>
    /// <param name="arg">The array or value to count</param>
    /// <returns>Number of items in the array</returns>
    [FunctionName("count")]
    public static int Count([AllowContextAsValue] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Undefined:
                return 0;
            case JTokenType.Array:
                return ((JArray)arg).Count;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Returns an array containing the values in array1 followed by the values in array2.
    /// If either parameter is not an array, then it is treated as a singleton array containing that value.
    /// </summary>
    /// <param name="array1">The first array or value</param>
    /// <param name="array2">The second array or value</param>
    /// <returns>Combined array</returns>
    [FunctionName("append")]
    public static JToken Append(JToken array1, JToken array2)
    {
        // disregard undefined args
        if (array1.Type == JTokenType.Undefined)
        {
            return array2;
        }
        else if (array2.Type == JTokenType.Undefined)
        {
            return array1;
        };
        // if either argument is not an array, make it so
        JArray result = new Sequence();
        if (array1.Type == JTokenType.Array)
        {
            result.AddRange(((JArray)array1).ChildrenTokens);
        }
        else
        {
            result.Add(array1);
        };
        if (array2.Type == JTokenType.Array)
        {
            result.AddRange(((JArray)array2).ChildrenTokens);
        }
        else
        {
            result.Add(array2);
        };
        return result;
    }

    /// <summary>
    /// Returns an array containing all the values in the array parameter, but sorted into order.
    /// If no function parameter is supplied, then the array parameter must contain only numbers or only strings, and they will be sorted in order of increasing number, or increasing unicode codepoint respectively.
    /// If a comparator function is supplied, then is must be a function that takes two parameters: function(left, right).
    /// This function gets invoked by the sorting algorithm to compare two values left and right.
    /// If the value of left should be placed after the value of right in the desired sort order, then the function must return Boolean true to indicate a swap. Otherwise it must return false.
    /// </summary>
    /// <param name="arrayToken">The array to sort</param>
    /// <param name="function">Optional comparator function</param>
    /// <returns>Sorted array</returns>
    [FunctionName("sort")]
    public static JArray Sort([PropagateUndefined] JToken arrayToken, [OptionalArgument(null)] JToken? function)
    {
        if (arrayToken.Type != JTokenType.Array)
        {
            Sequence singletonArray = new Sequence();
            singletonArray.keepSingletons = true;
            singletonArray.Add(arrayToken);
            return singletonArray;
        }

        JArray array = (JArray)arrayToken;
        if (array.Count <= 1)
        {
            return array;
        }

        System.Comparison<JToken> comparator;

        if (function == null || function.Type == JTokenType.Undefined)
        {
            if (array.IsArrayOfNumbers())
            {
                comparator = (a, b) => a.GetDoubleValue().CompareTo(b.GetDoubleValue());
            }
            else if (array.IsArrayOfStrings())
            {
                comparator = (a, b) => String.CompareOrdinal((string)a!, (string)b!);
            }
            else
            {
                throw new JsonataException("D3070", $"The single argument form of the {nameof(Sort)} function can only be applied to an array of strings or an array of numbers.  Use the second argument to specify a comparison function");
            }
        }
        else if (function.Type == JTokenType.Function)
        {
            comparator = (a, b) =>
            {
                JToken res = function.TryInvoke(
                    args: new List<JToken>() { a, b },
                    context: null,
                    env: null! //TODO: pass some real environment?
                );
                bool result = res.Booleanize();
                return result ? 1 : -1; //may cause problems because of no zero (
            };
        }
        else
        {
            //TODO: get proper code
            throw new JsonataException("????", $"Argument 2 of function {nameof(Sort)} should be a function(left, right) returning boolean");
        }

        List<JToken> tokens = array.ChildrenTokens.ToList();
        tokens.Sort(comparator);
        JArray result = new Sequence();
        result.AddRange(tokens);
        return result;
    }

    /// <summary>
    /// Returns an array containing all the values from the array parameter, but in reverse order.
    /// </summary>
    /// <param name="arrayToken">The array to reverse</param>
    /// <returns>Reversed array</returns>
    [FunctionName("reverse")]
    public static JArray Reverse([PropagateUndefined] JToken arrayToken)
    {
        if (arrayToken.Type != JTokenType.Array)
        {
            Sequence singletonArray = new Sequence();
            singletonArray.Add(arrayToken);
            return singletonArray;
        }

        JArray array = (JArray)arrayToken;
        if (array.Count <= 1)
        {
            return array;
        }

        JArray result = new JArray(array.Count);
        for (int i = array.Count - 1; i >= 0; --i)
        {
            result.Add(array.ChildrenTokens[i]);
        }
        return result;
    }

    /// <summary>
    /// Returns an array containing all the values from the array parameter, but shuffled into random order.
    /// </summary>
    /// <param name="arrayToken">The array to shuffle</param>
    /// <param name="evalEnv">The evaluation supplement (automatically provided)</param>
    /// <returns>Shuffled array</returns>
    [FunctionName("shuffle")]
    public static JArray Shuffle([PropagateUndefined] JToken arrayToken, [EvalSupplementArgument] EvaluationSupplement evalEnv)
    {
        if (arrayToken.Type != JTokenType.Array)
        {
            Sequence singletonArray = new Sequence();
            singletonArray.Add(arrayToken);
            return singletonArray;
        }

        JArray array = (JArray)arrayToken;
        if (array.Count <= 1)
        {
            return array;
        }
        JToken[] arr = new JToken[array.Count];
        for (int i = 0; i < array.Count; ++i)
        {
            arr[i] = array.ChildrenTokens[i];
        }
        for (int i = 0; i < arr.Length; ++i)
        {
            int j = evalEnv.Random.Next(i, arr.Length);
            if (i != j)
            {
                JToken tmp = arr[i];
                arr[i] = arr[j];
                arr[j] = tmp;
            }
        }
        JArray result = new JArray(arr.Length);
        for (int i = 0; i < arr.Length; ++i)
        {
            result.Add(arr[i]);
        }

        return result;
    }

    /// <summary>
    /// Returns an array containing all the values from the array parameter, but with any duplicates removed.
    /// Values are tested for deep equality as if by using the equality operator.
    /// </summary>
    /// <param name="arrayToken">The array to get distinct values from</param>
    /// <returns>Array with distinct values</returns>
    [FunctionName("distinct")]
    public static JArray Distinct([PropagateUndefined] JToken arrayToken)
    {
        if (arrayToken.Type != JTokenType.Array)
        {
            Sequence singletonArray = new Sequence();
            singletonArray.Add(arrayToken);
            return singletonArray;
        }

        JArray array = (JArray)arrayToken;
        if (array.Count <= 1)
        {
            return array;
        }

        JArray result = new JArray();
        foreach (JToken item in array.ChildrenTokens)
        {
            bool exists = false;
            foreach (JToken existing in result.ChildrenTokens)
            {
                if (DeepEquals(item, existing))
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                result.Add(item);
            }
        }
        return result;

        bool DeepEquals(JToken a, JToken b)
        {
            if (a.Type == JTokenType.Integer && b.Type == JTokenType.Float)
            {
                a = new JValue((double)(int)a);
            }
            else if (b.Type == JTokenType.Integer && b.Type == JTokenType.Float)
            {
                b = new JValue((double)(int)b);
            };
            return JToken.DeepEquals(a, b);
        }
    }

    /// <summary>
    /// Returns a convolved (zipped) array containing grouped arrays of values from the array1 ... arrayN arguments from index 0, 1, 2, etc.
    /// This function accepts a variable number of arguments.
    /// The length of the returned array is equal to the length of the shortest array in the arguments.
    /// </summary>
    /// <param name="args">Variable number of arrays to zip</param>
    /// <returns>Zipped array</returns>
    [FunctionName("zip")]
    public static JArray Zip([VariableNumberArgumentAsArray] JArray args)
    {
        JArray result = new JArray();
        int maxLength = int.MaxValue;
        List<JArray> argsList = new List<JArray>(args.Count);
        foreach (JToken arg in args.ChildrenTokens)
        {
            JArray arrayArg;
            switch (arg.Type)
            {
                case JTokenType.Undefined:
                    return result; //undefined has length of 0
                case JTokenType.Array:
                    arrayArg = (JArray)arg;
                    break;
                default:
                    arrayArg = new JArray(1);    //length of 1
                    arrayArg.Add(arg);
                    break;
            }
            argsList.Add(arrayArg);
            if (arrayArg.Count < maxLength)
            {
                maxLength = arrayArg.Count;
            }
        };

        for (int i = 0; i < maxLength; ++i)
        {
            JArray tuple = new JArray();
            foreach (JArray arrayArg in argsList)
            {
                tuple.Add(arrayArg.ChildrenTokens[i]);
            };
            result.Add(tuple);
        };

        return result;
    }

    /// <summary>
    /// Returns the arithmetic sum of an array of numbers.
    /// It is an error if the input array contains an item which isn't a number.
    /// </summary>
    /// <param name="arg">Array of numbers or a single number</param>
    /// <returns>Sum of the numbers</returns>
    [FunctionName("sum")]
    public static JToken Sum([PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                return arg;
            case JTokenType.Array:
                //continue handling below
                break;
            default:
                throw new JsonataException("T0410", $"Argument 1 of function {nameof(Sum)} should be an array of numbers, but specified {arg.Type}");
        }

        decimal result = ((JArray)arg).EnumerateNumericValues(nameof(Sum), 1).Sum();
        return FunctionToken.ReturnDecimalResult(result);
    }

    /// <summary>
    /// Returns the maximum number in an array of numbers.
    /// It is an error if the input array contains an item which isn't a number.
    /// </summary>
    /// <param name="arg">Array of numbers or a single number</param>
    /// <returns>Maximum number</returns>
    [FunctionName("max")]
    public static JToken Max([PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                return arg;
            case JTokenType.Array:
                //continue handling below
                break;
            default:
                throw new JsonataException("T0410", $"Argument 1 of function {nameof(Max)} should be an array of numbers, but specified {arg.Type}");
        }

        decimal result = Decimal.MinValue;
        bool found = false;
        foreach (decimal value in ((JArray)arg).EnumerateNumericValues(nameof(Max), 1))
        {
            if (value > result)
            {
                result = value;
            }
            found = true;
        };

        if (!found)
        {
            return EvalProcessor.UNDEFINED;
        }

        return FunctionToken.ReturnDecimalResult(result);
    }

    /// <summary>
    /// Returns the minimum number in an array of numbers.
    /// It is an error if the input array contains an item which isn't a number.
    /// </summary>
    /// <param name="arg">Array of numbers or a single number</param>
    /// <returns>Minimum number</returns>
    [FunctionName("min")]
    public static JToken Min([PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                return arg;
            case JTokenType.Array:
                //continue handling below
                break;
            default:
                throw new JsonataException("T0410", $"Argument 1 of function {nameof(Min)} should be an array of numbers, but specified {arg.Type}");
        }

        decimal result = Decimal.MaxValue;
        bool found = false;
        foreach (decimal value in ((JArray)arg).EnumerateNumericValues(nameof(Min), 1))
        {
            if (value < result)
            {
                result = value;
            }
            found = true;
        };

        if (!found)
        {
            return EvalProcessor.UNDEFINED;
        }

        return FunctionToken.ReturnDecimalResult(result);
    }

    /// <summary>
    /// Returns the mean value of an array of numbers. It is an error if the input array contains an item which isn't a number.
    /// </summary>
    /// <param name="arg">Array of numbers or a single number</param>
    /// <returns>Average of the numbers</returns>
    [FunctionName("average")]
    public static JToken Average([PropagateUndefined] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                return arg;
            case JTokenType.Array:
                //continue handling below
                break;
            default:
                throw new JsonataException("T0410", $"Argument 1 of function {nameof(Average)} should be an array of numbers, but specified {arg.Type}");
        }

        decimal result = 0;
        int count = 0;
        foreach (decimal value in ((JArray)arg).EnumerateNumericValues(nameof(Average), 1))
        {
            result += value;
            ++count;
        };

        if (count == 0)
        {
            return EvalProcessor.UNDEFINED;
        };
        return FunctionToken.ReturnDecimalResult(result / count);
    }
}