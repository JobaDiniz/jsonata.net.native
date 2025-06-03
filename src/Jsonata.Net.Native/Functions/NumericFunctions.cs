using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Eval;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Provides numeric operations and conversion functions for JSONata expressions.
/// </summary>
public static class NumericFunctions
{
    /// <summary>
    /// Casts the arg parameter to a number using JSONata casting rules.
    /// If arg is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of arg.
    /// </summary>
    /// <param name="arg">The value to convert to a number</param>
    /// <returns>Numeric representation of the input value</returns>
    [FunctionName("number")]
    public static JToken ToNumber([AllowContextAsValue] JToken arg)
    {
        switch (arg.Type)
        {
            case JTokenType.Undefined:
                // undefined inputs always return undefined
                return arg;
            case JTokenType.Integer:
                //Numbers are unchanged
                return arg;
            case JTokenType.Float:
                //Numbers are unchanged
                return arg;
            case JTokenType.String:
                //Strings that contain a sequence of characters that represent a legal JSON number are converted to that number
                {
                    string str = (string)arg!;
                    if (Int64.TryParse(str, out long longValue))
                    {
                        return new JValue(longValue);
                    }
                    else if (Double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                    {
                        if (!Double.IsNaN(doubleValue) && !Double.IsInfinity(doubleValue))
                        {
                            long doubleAsLongValue = (long)doubleValue;
                            if (doubleAsLongValue == doubleValue)
                            {
                                //support for 1e5 cases
                                return new JValue(doubleAsLongValue);
                            }
                            else
                            {
                                return new JValue(doubleValue);
                            }
                        }
                        else
                        {
                            throw new JsonataException("D3030", "Jsonata does not support NaNs or Infinity values");
                        }
                    }
                    else
                    {
                        throw new JsonataException("D3030", $"Failed to parse string to number: '{str}'");
                    }
                };
            case JTokenType.Boolean:
                //Boolean true converts to 1, false converts to 0
                return new JValue((bool)arg ? 1 : 0);
            default:
                //All other values cause an error to be thrown.
                throw new JsonataException("D3030", $"Unable to cast value to a number. Value type is {arg.Type}");
        }
    }

    /// <summary>
    /// Returns the absolute value of the number parameter, i.e. if the number is negative, it returns the positive value.
    /// If number is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of number.
    /// </summary>
    /// <param name="number">The number to get the absolute value of</param>
    /// <returns>The absolute value of the number</returns>
    [FunctionName("abs")]
    public static double Abs([AllowContextAsValue][PropagateUndefined] double number)
    {
        return Math.Abs(number);
    }

    /// <summary>
    /// Returns the value of number rounded down to the nearest integer that is smaller or equal to number.
    /// If number is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of number.
    /// </summary>
    /// <param name="number">The number to floor</param>
    /// <returns>The floor of the number</returns>
    [FunctionName("floor")]
    public static long Floor([AllowContextAsValue][PropagateUndefined] double number)
    {
        return (long)Math.Floor(number);
    }

    /// <summary>
    /// Returns the value of number rounded up to the nearest integer that is greater than or equal to number.
    /// If number is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of number.
    /// </summary>
    /// <param name="number">The number to ceiling</param>
    /// <returns>The ceiling of the number</returns>
    [FunctionName("ceil")]
    public static long Ceil([AllowContextAsValue][PropagateUndefined] double number)
    {
        return (long)Math.Ceiling(number);
    }

    /// <summary>
    /// Returns the value of the number parameter rounded to the number of decimal places specified by the optional precision parameter.
    /// The precision parameter (which must be an integer) species the number of decimal places to be present in the rounded number.
    /// If precision is not specified then it defaults to the value 0 and the number is rounded to the nearest integer.
    /// If precision is negative, then its value specifies which column to round to on the left side of the decimal place.
    /// This function uses the Round half to even strategy to decide which way to round numbers that fall exactly between two candidates at the specified precision.
    /// This strategy is commonly used in financial calculations and is the default rounding mode in IEEE 754.
    /// </summary>
    /// <param name="number">The number to round</param>
    /// <param name="precision">The number of decimal places (default 0)</param>
    /// <returns>The rounded number</returns>
    [FunctionName("round")]
    public static decimal Round([AllowContextAsValue][PropagateUndefined] decimal number, [OptionalArgument(0)] int precision)
    {
        //This function uses decimal because Math.Round for double in C# does not exactly follow the expectations because of binary arithmetics issues
        if (precision >= 0)
        {
            return Math.Round(number, precision, MidpointRounding.ToEven);
        }
        else
        {
            precision = -precision;
            int power = (int)Math.Pow(10, precision);
            number = Math.Round(number / power, 0, MidpointRounding.ToEven);
            number *= power;
            return number;
        }
    }

    /// <summary>
    /// Returns the value of base raised to the power of exponent (base ^ exponent).
    /// If base is not specified (i.e. this function is invoked with one argument), then the context value is used as the value of base.
    /// An error is thrown if the values of base and exponent lead to a value that cannot be represented as a JSON number (e.g. Infinity, complex numbers).
    /// </summary>
    /// <param name="baseValue">The base number</param>
    /// <param name="exponent">The exponent</param>
    /// <returns>The result of base raised to the power of exponent</returns>
    [FunctionName("power")]
    public static double Power([AllowContextAsValue][PropagateUndefined] double baseValue, double exponent)
    {
        return Math.Pow(baseValue, exponent);
    }

    /// <summary>
    /// Returns the square root of the value of the number parameter.
    /// If number is not specified (i.e. this function is invoked with one argument), then the context value is used as the value of number.
    /// An error is thrown if the value of number is negative.
    /// </summary>
    /// <param name="number">The number to get the square root of</param>
    /// <returns>The square root of the number</returns>
    [FunctionName("sqrt")]
    public static double Sqrt([AllowContextAsValue][PropagateUndefined] double number)
    {
        return Math.Sqrt(number);
    }

    /// <summary>
    /// Returns a pseudo random number greater than or equal to zero and less than one (0 ≤ n &lt; 1).
    /// </summary>
    /// <param name="executionState">The query execution state (automatically provided)</param>
    /// <returns>A random number between 0 and 1</returns>
    [FunctionName("random")]
    public static double Random([ExecutionStateArgument] QueryExecutionState executionState)
    {
        return executionState.Random.NextDouble();
    }

    /// <summary>
    /// Casts the number to a string and formats it to a decimal representation as specified by the picture string.
    /// The behaviour of this function is consistent with the XPath/XQuery function fn:format-number as defined in the XPath F&O 3.1 specification.
    /// The picture string parameter defines how the number is formatted and has the same syntax as fn:format-number.
    /// The optional third argument options is used to override the default locale specific formatting characters such as the decimal separator.
    /// If supplied, this argument must be an object containing name/value pairs specified in the decimal format section of the XPath F&O 3.1 specification.
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="picture">The format picture string</param>
    /// <param name="options">Optional formatting options</param>
    /// <returns>Formatted number string</returns>
    [FunctionName("formatNumber")]
    public static string FormatNumber([AllowContextAsValue][PropagateUndefined] double number, string picture, [OptionalArgument(null)] JObject? options)
    {
        //TODO: try implementing or using proper XPath fn:format-number
        picture = Regex.Replace(picture, @"[1-9]", "0");
        picture = Regex.Replace(picture, @",", @"\,");
        return number.ToString(picture, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Casts the number to a string and formats it to an integer represented in the number base specified by the radix argument.
    /// If radix is not specified, then it defaults to base 10. radix can be between 2 and 36, otherwise an error is thrown.
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="radix">The number base (2-36, default 10)</param>
    /// <returns>String representation of the number in the specified base</returns>
    [FunctionName("formatBase")]
    public static string FormatBase([AllowContextAsValue][PropagateUndefined] long number, [OptionalArgument(10)] int radix)
    {
        if (radix < 2 || radix > 36)
        {
            throw new JsonataException("D3100", $"The radix of the {nameof(FormatBase)} function must be between 2 and 36. It was given {radix}");
        };

        //TODO: implement properly
        if (number < 0)
        {
            return "-" + FormatBase(-number, radix);
        };
        switch (radix)
        {
            case 2:
            case 8:
            case 10:
            case 16:
                return Convert.ToString(number, radix);
            default:
                throw new NotImplementedException($"No support for radix={radix} in {nameof(FormatBase)}() yet");
        }
    }

    /// <summary>
    /// Casts the number to a string and formats it to an integer representation as specified by the picture string.
    /// The behaviour of this function is consistent with the two-argument version of the XPath/XQuery function fn:format-integer as defined in the XPath F&O 3.1 specification.
    /// The picture string parameter defines how the number is formatted and has the same syntax as fn:format-integer.
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="picture">The format picture string</param>
    /// <returns>Formatted integer string</returns>
    [FunctionName("formatInteger")]
    public static string FormatInteger([AllowContextAsValue][PropagateUndefined] long number, string picture)
    {
        //TODO: try implementing or using proper XPath fn:format-integer
        //picture = Regex.Replace(picture, @"[1-9]", "0");
        //picture = Regex.Replace(picture, @",", @"\,");
        return number.ToString(picture, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses the contents of the string parameter to an integer (as a JSON number) using the format specified by the picture string.
    /// The picture string parameter has the same format as $formatInteger.
    /// Although the XPath specification does not have an equivalent function for parsing integers, this capability has been added to JSONata.
    /// </summary>
    /// <param name="str">The string to parse</param>
    /// <param name="picture">Optional format picture string</param>
    /// <returns>Parsed integer value</returns>
    [FunctionName("parseInteger")]
    public static long ParseInteger([AllowContextAsValue][PropagateUndefined] string str, [OptionalArgument(null)] string? picture)
    {
        //TODO: try implementing properly
        return Int64.Parse(str);
    }
}