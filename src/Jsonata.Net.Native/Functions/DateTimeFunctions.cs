using Jsonata.Net.Native.Eval;
using System;
using System.Globalization;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Provides date and time manipulation functions for JSONata expressions.
/// </summary>
public static class DateTimeFunctions
{
    internal const string UTC_FORMAT = @"yyyy-MM-dd\THH:mm:ss.fffK";

    /// <summary>
    /// Generates a UTC timestamp in ISO 8601 compatible format and returns it as a string. All invocations of $now() within an evaluation of an expression will all return the same timestamp value.
    /// If the optional picture and timezone parameters are supplied, then the current timestamp is formatted as described by the $fromMillis() function.
    /// </summary>
    /// <param name="picture">Optional format picture string</param>
    /// <param name="timezone">Optional timezone offset</param>
    /// <param name="evalEnv">The evaluation supplement (automatically provided)</param>
    /// <returns>Current timestamp as formatted string</returns>
    [FunctionName("now")]
    public static string Now([OptionalArgument(UTC_FORMAT)] string picture, [OptionalArgument(null)] string? timezone, [EvalSupplementArgument] EvaluationSupplement evalEnv)
    {
        return FromMillis(Millis(evalEnv), picture, timezone);
    }

    /// <summary>
    /// Returns the number of milliseconds since the Unix Epoch (1 January, 1970 UTC) as a number.
    /// All invocations of $millis() within an evaluation of an expression will all return the same value.
    /// </summary>
    /// <param name="evalEnv">The evaluation supplement (automatically provided)</param>
    /// <returns>Milliseconds since Unix Epoch</returns>
    [FunctionName("millis")]
    public static long Millis([EvalSupplementArgument] EvaluationSupplement evalEnv)
    {
        return evalEnv.Now.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Convert the number representing milliseconds since the Unix Epoch (1 January, 1970 UTC) to a formatted string representation of the timestamp as specified by the picture string.
    /// If the optional picture parameter is omitted, then the timestamp is formatted in the ISO 8601 format.
    /// If the optional picture string is supplied, then the timestamp is formatted occording to the representation specified in that string. The behaviour of this function is consistent with the two-argument version of the XPath/XQuery function fn:format-dateTime as defined in the XPath F&O 3.1 specification. The picture string parameter defines how the timestamp is formatted and has the same syntax as fn:format-dateTime.
    /// If the optional timezone string is supplied, then the formatted timestamp will be in that timezone. The timezone string should be in the format "±HHMM", where ± is either the plus or minus sign and HHMM is the offset in hours and minutes from UTC. Positive offset for timezones east of UTC, negative offset for timezones west of UTC.
    /// </summary>
    /// <param name="number">Milliseconds since Unix Epoch</param>
    /// <param name="picture">Optional format picture string</param>
    /// <param name="timezone">Optional timezone offset</param>
    /// <returns>Formatted timestamp string</returns>
    [FunctionName("fromMillis")]
    public static string FromMillis([PropagateUndefined] long number, [OptionalArgument(UTC_FORMAT)] string picture, [OptionalArgument(null)] string? timezone)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(number);
        if (timezone != null)
        {
            if (!Int32.TryParse(timezone, out int offsetHhMm))
            {
                throw new JsonataException("D3134", $"Failed to parse timezone offset value from '{timezone}'");
            }
            date = date.ToOffset(new TimeSpan(offsetHhMm / 100, offsetHhMm % 100, 0));
        }
        //see how "K" different for DateTime and DateTimeOffset https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#KSpecifier
        return (new DateTime(date.Ticks, DateTimeKind.Utc)).ToString(picture);
    }

    /// <summary>
    /// Convert a timestamp string to the number of milliseconds since the Unix Epoch (1 January, 1970 UTC) as a number.
    /// If the optional picture string is not specified, then the format of the timestamp is assumed to be ISO 8601.
    /// An error is thrown if the string is not in the correct format.
    /// If the picture string is specified, then the format is assumed to be described by this picture string using the same syntax as the XPath/XQuery function fn:format-dateTime, defined in the XPath F&O 3.1 specification.
    /// </summary>
    /// <param name="timestamp">The timestamp string to parse</param>
    /// <param name="picture">Optional format picture string</param>
    /// <returns>Milliseconds since Unix Epoch</returns>
    [FunctionName("toMillis")]
    public static long ToMillis([PropagateUndefined] string timestamp, [OptionalArgument(null)] string? picture)
    {
        DateTimeOffset result;
        if (picture == null)
        {
            if (!DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
            {
                throw new JsonataException("D3136", $"Failed to parse date/time from '{timestamp}'");
            }
        }
        else
        {
            if (!DateTimeOffset.TryParseExact(timestamp, picture, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
            {
                throw new JsonataException("D3136", $"Failed to parse date/time from '{timestamp}' using picture format '{picture}'");
            }
        }

        return result.ToUnixTimeMilliseconds();
    }
}