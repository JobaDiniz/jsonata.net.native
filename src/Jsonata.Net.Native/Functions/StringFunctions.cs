using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Provides string manipulation and conversion functions for JSONata expressions.
/// </summary>
public static class StringFunctions
{
    private static readonly Encoding UTF8_NO_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Casts the arg parameter to a string using JSONata casting rules.
    /// If arg is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of arg.
    /// If prettify is true, then "prettified" JSON is produced. i.e One line per field and lines will be indented based on the field depth.
    /// </summary>
    /// <param name="arg">The value to convert to string</param>
    /// <param name="prettify">Whether to produce prettified JSON output</param>
    /// <returns>String representation of the input value</returns>
    [FunctionName("string")]
    public static JToken ToString([AllowContextAsValue] JToken arg, [OptionalArgument(false)] bool prettify)
    {
        switch (arg.Type)
        {
            case JTokenType.Undefined:
                // undefined inputs always return undefined
                return arg;
            case JTokenType.String:
                //Strings are unchanged
                return arg;
            case JTokenType.Float:
                {
                    double value = (double)arg;
                    if (Double.IsNaN(value) || Double.IsInfinity(value))
                    {
                        throw new JsonataException("D3001", "Attempting to invoke string function on Infinity or NaN");
                    };
                    return new JValue(arg.ToFlatString());
                };
            case JTokenType.Function:
                //Functions are converted to an empty string
                return new JValue("");
            default:
                return new JValue(prettify ? arg.ToIndentedString() : arg.ToFlatString());
        }
    }

    /// <summary>
    /// Returns the number of characters in the string str.
    /// If str is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of str.
    /// An error is thrown if str is not a string.
    /// </summary>
    /// <param name="str">The string to measure</param>
    /// <returns>Number of characters in the string</returns>
    [FunctionName("length")]
    public static int Length([AllowContextAsValue][PropagateUndefined] string str)
    {
        return str.Length;
    }

    /// <summary>
    /// Returns a string containing the characters in the first parameter str starting at position start (zero-offset).
    /// If str is not specified (i.e. this function is invoked with only the numeric argument(s)), then the context value is used as the value of str. An error is thrown if str is not a string.
    /// If length is specified, then the substring will contain maximum length characters.
    /// If start is negative then it indicates the number of characters from the end of str. See substr for full definition.
    /// </summary>
    /// <param name="str">The source string</param>
    /// <param name="start">Starting position (zero-based)</param>
    /// <param name="len">Maximum length of substring</param>
    /// <returns>Substring of the input string</returns>
    [FunctionName("substring")]
    public static string Substring([AllowContextAsValue][PropagateUndefined] string str, int start, [OptionalArgument(100000000)] int len)
    {
        //see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/substr
        if (start < 0)
        {
            start = str.Length + start;
            if (start < 0)
            {
                start = 0;
            }
        };
        if (start + len > str.Length)
        {
            len = str.Length - start;
        };
        if (len < 0)
        {
            len = 0;
        };
        return str.Substring(start, len);
    }

    /// <summary>
    /// Returns the substring before the first occurrence of the character sequence chars in str.
    /// If str is not specified (i.e. this function is invoked with only one argument), then the context value is used as the value of str.
    /// If str does not contain chars, then it returns str. An error is thrown if str and chars are not strings.
    /// </summary>
    /// <param name="str">The source string</param>
    /// <param name="chars">The character sequence to search for</param>
    /// <returns>Substring before the first occurrence of chars</returns>
    [FunctionName("substringBefore")]
    public static string SubstringBefore([AllowContextAsValue][PropagateUndefined] string str, string chars)
    {
        int index = str.IndexOf(chars);
        if (index < 0)
        {
            return str;
        }
        else
        {
            return str.Substring(0, index);
        }
    }

    /// <summary>
    /// Returns the substring after the first occurrence of the character sequence chars in str.
    /// If str is not specified (i.e. this function is invoked with only one argument), then the context value is used as the value of str.
    /// If str does not contain chars, then it returns str. An error is thrown if str and chars are not strings.
    /// </summary>
    /// <param name="str">The source string</param>
    /// <param name="chars">The character sequence to search for</param>
    /// <returns>Substring after the first occurrence of chars</returns>
    [FunctionName("substringAfter")]
    public static string SubstringAfter([AllowContextAsValue][PropagateUndefined] string str, string chars)
    {
        int index = str.IndexOf(chars);
        if (index < 0)
        {
            return str;
        }
        else
        {
            return str.Substring(index + chars.Length);
        }
    }

    /// <summary>
    /// Returns a string with all the characters of str converted to uppercase.
    /// If str is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of str.
    /// An error is thrown if str is not a string.
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>String with all characters converted to uppercase</returns>
    [FunctionName("uppercase")]
    public static string Uppercase([AllowContextAsValue][PropagateUndefined] string str)
    {
        return str.ToUpper();
    }

    /// <summary>
    /// Returns a string with all the characters of str converted to lowercase.
    /// If str is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of str.
    /// An error is thrown if str is not a string.
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>String with all characters converted to lowercase</returns>
    [FunctionName("lowercase")]
    public static string Lowercase([AllowContextAsValue][PropagateUndefined] string str)
    {
        return str.ToLower();
    }

    /// <summary>
    /// Normalizes and trims all whitespace characters in str by applying the following steps:
    /// All tabs, carriage returns, and line feeds are replaced with spaces.
    /// Contiguous sequences of spaces are reduced to a single space.
    /// Trailing and leading spaces are removed.
    /// If str is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of str.
    /// An error is thrown if str is not a string.
    /// </summary>
    /// <param name="str">The string to trim</param>
    /// <returns>Normalized and trimmed string</returns>
    [FunctionName("trim")]
    public static string Trim([AllowContextAsValue][PropagateUndefined] string str)
    {
        str = Regex.Replace(str, @"\s+", " ");
        return str.Trim();
    }

    /// <summary>
    /// Returns a copy of the string str with extra padding, if necessary, so that its total number of characters is at least the absolute value of the width parameter.
    /// If width is a positive number, then the string is padded to the right;
    /// if negative, it is padded to the left.
    /// The optional char argument specifies the padding character(s) to use.
    /// If not specified, it defaults to the space character.
    /// </summary>
    /// <param name="str">The string to pad</param>
    /// <param name="width">The target width (positive for right padding, negative for left padding)</param>
    /// <param name="chars">The padding characters to use</param>
    /// <returns>Padded string</returns>
    [FunctionName("pad")]
    public static string Pad([AllowContextAsValue][PropagateUndefined] string str, int width, [OptionalArgument(" ")] string chars)
    {
        if (chars == "")
        {
            chars = " ";
        };

        if (width >= 0)
        {
            bool changed = false;
            while (str.Length < width)
            {
                str += chars;
                changed = true;
            };
            if (changed && str.Length > width)
            {
                str = str.Substring(0, width);
            };
        }
        else
        {
            width = -width;
            bool changed = false;
            while (str.Length < width)
            {
                str = chars + str;
                changed = true;
            };
            if (changed && str.Length > width)
            {
                str = str.Substring(str.Length - width);
            };
        };

        return str;
    }

    /// <summary>
    /// Returns true if str is matched by pattern, otherwise it returns false.
    /// If str is not specified (i.e. this function is invoked with one argument), then the context value is used as the value of str.
    /// The pattern parameter can either be a string or a regular expression (regex).
    /// If it is a string, the function returns true if the characters within pattern are contained contiguously within str.
    /// If it is a regex, the function will return true if the regex matches the contents of str.
    /// </summary>
    /// <param name="str">The string to search in</param>
    /// <param name="pattern">The pattern to search for (string or regex)</param>
    /// <returns>True if the pattern is found, false otherwise</returns>
    [FunctionName("contains")]
    public static bool Contains([AllowContextAsValue][PropagateUndefined] string str, JToken pattern)
    {
        switch (pattern.Type)
        {
            case JTokenType.String:
                return str.Contains((string)pattern!);
            case JTokenType.Function:
                if (pattern is not FunctionTokenRegex regex)
                {
                    throw new JsonataException("T0410", $"Argument 2 of function {nameof(Contains)} should be either string or regex. Passed function {pattern.GetType().Name})");
                }
                else
                {
                    return regex.regex.IsMatch(str);
                }
            default:
                throw new JsonataException("T0410", $"Argument 2 of function {nameof(Contains)} should be either string or regex. Passed {pattern.Type} ({pattern.ToFlatString()})");
        }
    }

    /// <summary>
    /// Splits the str parameter into an array of substrings.
    /// If str is not specified, then the context value is used as the value of str.
    /// It is an error if str is not a string.
    /// The separator parameter can either be a string or a regular expression (regex).
    /// If it is a string, it specifies the characters within str about which it should be split.
    /// If it is the empty string, str will be split into an array of single characters.
    /// If it is a regex, it splits the string around any sequence of characters that match the regex.
    /// The optional limit parameter is a number that specifies the maximum number of substrings to include in the resultant array.
    /// Any additional substrings are discarded.
    /// If limit is not specified, then str is fully split with no limit to the size of the resultant array.
    /// It is an error if limit is not a non-negative number.
    /// </summary>
    /// <param name="str">The string to split</param>
    /// <param name="separator">The separator (string or regex)</param>
    /// <param name="limit">Maximum number of substrings</param>
    /// <returns>Array of substrings</returns>
    [FunctionName("split")]
    public static JArray Split([PropagateUndefined] string str, JToken separator, [OptionalArgument(Int32.MaxValue)] int limit)
    {
        //TODO: support RegExes!!

        if (limit < 0)
        {
            throw new JsonataException("D3020", $"Third argument of {nameof(Split)} function must evaluate to a positive number. Passed {limit}");
        }

        JArray result = new JArray();

        switch (separator.Type)
        {
            case JTokenType.String:
                {
                    string separatorString = (string)separator!;
                    if (separatorString == "")
                    {
                        foreach (char c in str)
                        {
                            if (result.Count >= limit)
                            {
                                break;
                            }
                            result.Add(new JValue(c));
                        }
                    }
                    else
                    {
                        foreach (string part in Regex.Split(str, Regex.Escape(separatorString)))
                        {
                            if (result.Count >= limit)
                            {
                                break;
                            }
                            result.Add(new JValue(part));
                        }
                    }
                }
                break;
            case JTokenType.Function:
                {
                    if (separator is not FunctionTokenRegex regex)
                    {
                        throw new JsonataException("T0410", $"Argument 2 of function {nameof(Split)} should be either string or regex. Passed function {separator.GetType().Name})");
                    };
                    foreach (string part in regex.regex.Split(str))
                    {
                        if (result.Count >= limit)
                        {
                            break;
                        }
                        result.Add(new JValue(part));
                    };
                }
                break;
            default:
                throw new JsonataException("T0410", $"Argument 2 of function {nameof(Split)} should be either string or regex. Passed {separator.Type} ({separator.ToFlatString()})");
        }
        return result;
    }

    /// <summary>
    /// Joins an array of component strings into a single concatenated string with each component string separated by the optional separator parameter.
    /// It is an error if the input array contains an item which isn't a string.
    /// If separator is not specified, then it is assumed to be the empty string, i.e. no separator between the component strings.
    /// It is an error if separator is not a string.
    /// </summary>
    /// <param name="array">Array of strings to join</param>
    /// <param name="separator">Optional separator string</param>
    /// <returns>Joined string</returns>
    [FunctionName("join")]
    public static string Join([PropagateUndefined] JToken array, [OptionalArgument(null)] JToken? separator)
    {
        string separatorString;
        if (separator == null)
        {
            separatorString = "";
        }
        else
        {
            switch (separator.Type)
            {
                case JTokenType.Undefined:
                    separatorString = "";
                    break;
                case JTokenType.String:
                    separatorString = (string)separator!;
                    break;
                default:
                    throw new JsonataException("T0410", $"Argument 2 of function {nameof(Join)} is expected to be string. Specified {separator.Type}");
            }
        };

        List<string> elements = new List<string>();
        switch (array.Type)
        {
            case JTokenType.String:
                elements.Add((string)array!);
                break;
            case JTokenType.Array:
                foreach (JToken element in ((JArray)array).ChildrenTokens)
                {
                    if (element.Type != JTokenType.String)
                    {
                        throw new JsonataException("T0412", $"Argument 1 of function {nameof(Join)} must be an array of strings");
                    }
                    else
                    {
                        elements.Add((string)element!);
                    }
                }
                break;
            default:
                throw new JsonataException("T0410", $"Argument 1 of function {nameof(Join)} is expected to be an Array. Specified {array.Type}");
        }
        return String.Join(separatorString, elements);
    }

    /// <summary>
    /// Applies the str string to the pattern regular expression and returns an array of objects,
    /// with each object containing information about each occurrence of a match within str.
    /// The object contains the following fields:
    /// match - the substring that was matched by the regex.
    /// index - the offset (starting at zero) within str of this match.
    /// groups - if the regex contains capturing groups (parentheses), this contains an array of strings representing each captured group.
    /// If str is not specified, then the context value is used as the value of str. It is an error if str is not a string.
    /// </summary>
    /// <param name="str">The string to match against</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <param name="limit">Maximum number of matches</param>
    /// <returns>Array of match objects</returns>
    [FunctionName("match")]
    public static JArray Match([AllowContextAsValue][PropagateUndefined] string str, JToken pattern, [OptionalArgument(Int32.MaxValue)] int limit)
    {
        if (pattern is not FunctionTokenRegex regex)
        {
            throw new JsonataException("T0410", $"Argument 2 of function {nameof(Match)} should be regex. Passed {pattern.Type} ({pattern.ToFlatString()})");
        };

        if (limit < 0)
        {
            throw new JsonataException("D3040", $"Third argument of {nameof(Match)} function must evaluate to a positive number");
        };

        JArray result = new JArray();
        foreach (Match match in regex.regex.Matches(str))
        {
            if (result.Count >= limit)
            {
                break;
            };
            result.Add(FunctionTokenRegex.ConvertRegexMatch(match));
        }

        return result;
    }

    /// <summary>
    /// Finds occurrences of pattern within str and replaces them with replacement.
    /// If str is not specified, then the context value is used as the value of str. It is an error if str is not a string.
    /// The pattern parameter can either be a string or a regular expression (regex).
    /// If it is a string, it specifies the substring(s) within str which should be replaced.
    /// If it is a regex, its is used to find matches.
    /// The replacement parameter can either be a string or a function.
    /// If it is a string, it specifies the sequence of characters that replace the substring(s) that are matched by pattern.
    /// If pattern is a regex, then the replacement string can refer to the characters that were matched by the regex as well as any of the captured groups
    /// using a $ followed by a number N:
    /// If N = 0, then it is replaced by substring matched by the regex as a whole.
    /// If N > 0, then it is replaced by the substring captured by the Nth parenthesised group in the regex.
    /// If N is greater than the number of captured groups, then it is replaced by the empty string.
    /// A literal $ character must be written as $$ in the replacement string
    /// If the replacement parameter is a function, then it is invoked for each match occurrence of the pattern regex.
    /// The replacement function must take a single parameter which will be the object structure of a regex match
    /// as described in the $match function; and must return a string.
    /// The optional limit parameter, is a number that specifies the maximum number of replacements to make before stopping.
    /// The remainder of the input beyond this limit will be copied to the output unchanged.
    /// </summary>
    /// <param name="str">The string to search in</param>
    /// <param name="pattern">The pattern to search for</param>
    /// <param name="replacement">The replacement value</param>
    /// <param name="limit">Maximum number of replacements</param>
    /// <returns>String with replacements made</returns>
    [FunctionName("replace")]
    public static string Replace([PropagateUndefined] string str, JToken pattern, JToken replacement, [OptionalArgument(Int32.MaxValue)] int limit)
    {
        if (limit < 0)
        {
            throw new JsonataException("D3011", $"Fourth argument of {nameof(Replace)} function must evaluate to a positive number");
        }
        else if (limit == 0)
        {
            return str;
        }

        switch (pattern.Type)
        {
            case JTokenType.String:
                {
                    string patternString = (string)pattern!;
                    if (patternString == "")
                    {
                        throw new JsonataException("D3010", $"Second argument of {nameof(Replace)} function cannot be an empty string");
                    }
                    else
                    {
                        if (replacement.Type != JTokenType.String)
                        {
                            throw new JsonataException("D3012", "Attempted to replace a matched string with a non-string value");
                        };
                        string replacementString = (string)replacement!;
                        StringBuilder builder = new StringBuilder();
                        int replacesCount = 0;
                        int replaceStartAt = 0;
                        while (true)
                        {
                            if (replacesCount >= limit)
                            {
                                break;
                            };
                            int pos = str.IndexOf(patternString, startIndex: replaceStartAt);
                            if (pos < 0)
                            {
                                break;
                            }
                            else
                            {
                                if (pos > replaceStartAt)
                                {
                                    builder.Append(str.Substring(replaceStartAt, pos - replaceStartAt));
                                }
                                builder.Append(replacementString);
                                ++replacesCount;
                                replaceStartAt = pos + patternString.Length;
                            };
                        }
                        if (replaceStartAt < str.Length)
                        {
                            builder.Append(str.Substring(replaceStartAt));
                        };
                        return builder.ToString();
                    }
                }
            case JTokenType.Function:
                {
                    if (pattern is not FunctionTokenRegex regex)
                    {
                        throw new JsonataException("T0410", $"Argument 2 of function {nameof(Replace)} should be either string or regex. Passed function {pattern.GetType().Name})");
                    };

                    MatchCollection matches = regex.regex.Matches(str);
                    if (matches.Count == 0)
                    {
                        return str;
                    };

                    switch (replacement.Type)
                    {
                        case JTokenType.String:
                            {
                                string replacementString = (string)replacement!;
                                StringBuilder builder = new StringBuilder();
                                int replacesCount = 0;
                                int replaceStartAt = 0;
                                foreach (Match match in matches)
                                {
                                    if (replacesCount >= limit)
                                    {
                                        break;
                                    };
                                    if (match.Index < replaceStartAt)
                                    {
                                        continue;   //overlapping matches
                                    }
                                    else if (match.Index > replaceStartAt)
                                    {
                                        builder.Append(str.Substring(replaceStartAt, match.Index - replaceStartAt));
                                    }
                                    //TODO: use ProcessAppendReplacementStringForMatch instead of Result, but actually it's too ugly!
                                    //ProcessAppendReplacementStringForMatch(builder, match, replacementString);
                                    builder.Append(match.Result(replacementString));
                                    ++replacesCount;
                                    replaceStartAt = match.Index + match.Length;
                                }
                                if (replaceStartAt < str.Length)
                                {
                                    builder.Append(str.Substring(replaceStartAt));
                                };
                                return builder.ToString();
                            }
                        case JTokenType.Function:
                            {
                                FunctionToken replacementFunction = (FunctionToken)replacement;
                                StringBuilder builder = new StringBuilder();
                                EvaluationEnvironment env = EvaluationEnvironment.CreateEvalEnvironment(EvaluationEnvironment.DefaultEnvironment); //TODO: think of providing proper env. Maybe via a func param?
                                int replacesCount = 0;
                                int replaceStartAt = 0;
                                foreach (Match match in matches)
                                {
                                    if (replacesCount >= limit)
                                    {
                                        break;
                                    };
                                    if (match.Index < replaceStartAt)
                                    {
                                        continue;   //overlapping matches
                                    }
                                    else if (match.Index > replaceStartAt)
                                    {
                                        builder.Append(str.Substring(replaceStartAt, match.Index - replaceStartAt));
                                    };
                                    JObject matchObject = FunctionTokenRegex.ConvertRegexMatch(match);
                                    JToken replacementToken = ((JToken)replacementFunction).TryInvoke(new List<JToken>() { matchObject }, null, env);
                                    if (replacementToken.Type != JTokenType.String)
                                    {
                                        throw new JsonataException("D3012", "Attempted to replace a matched string with a non-string value");
                                    }
                                    builder.Append((string)replacementToken!);
                                    ++replacesCount;
                                    replaceStartAt = match.Index + match.Length;
                                }
                                if (replaceStartAt < str.Length)
                                {
                                    builder.Append(str.Substring(replaceStartAt));
                                };
                                return builder.ToString();
                            }
                        default:
                            throw new JsonataException("T0410", $"Argument 3 of function {nameof(Replace)} should be either string or function. Passed {replacement.Type} ({replacement.ToFlatString()})");
                    };
                }
            default:
                throw new JsonataException("T0410", $"Argument 2 of function {nameof(Replace)} should be either string or regex. Passed {pattern.Type} ({pattern.ToFlatString()})");
        };
    }

    /// <summary>
    /// Parses and evaluates the string expr which contains literal JSON or a JSONata expression using the current context as the context for evaluation.
    /// Optionally override the context by specifying the second parameter.
    /// </summary>
    /// <param name="expr">The expression to evaluate</param>
    /// <param name="context">Optional context to use for evaluation</param>
    /// <returns>Result of the evaluated expression</returns>
    [FunctionName("eval")]
    public static JToken Eval([PropagateUndefined] string expr, [AllowContextAsValue] JToken context)
    {
        JsonataQuery query = new JsonataQuery(expr);
        return query.Eval(context);    //TODO: think of using bindings from current environment (custom bindings). Also propagating time from parentevaluationEnvironment
    }

    /// <summary>
    /// Converts an ASCII string to a base 64 representation.
    /// Each each character in the string is treated as a byte of binary data.
    /// This requires that all characters in the string are in the 0x00 to 0xFF range, which includes all characters in URI encoded strings.
    /// Unicode characters outside of that range are not supported.
    /// </summary>
    /// <param name="str">The string to encode</param>
    /// <returns>Base64 encoded string</returns>
    [FunctionName("base64encode")]
    public static string Base64Encode([AllowContextAsValue][PropagateUndefined] string str)
    {
        return Convert.ToBase64String(UTF8_NO_BOM.GetBytes(str));
    }

    /// <summary>
    /// Converts base 64 encoded bytes to a string, using a UTF-8 Unicode codepage.
    /// </summary>
    /// <param name="str">The base64 string to decode</param>
    /// <returns>Decoded string</returns>
    [FunctionName("base64decode")]
    public static string Base64Decode([AllowContextAsValue][PropagateUndefined] string str)
    {
        return UTF8_NO_BOM.GetString(Convert.FromBase64String(str));
    }

    /// <summary>
    /// Encodes a Uniform Resource Locator(URL) component by replacing each instance of certain characters by one, two, three, or four escape sequences representing the UTF-8 encoding of the character.
    /// </summary>
    /// <param name="str">The string to encode</param>
    /// <returns>URL component encoded string</returns>
    [FunctionName("encodeUrlComponent")]
    public static string EncodeUrlComponent([AllowContextAsValue][PropagateUndefined] string str)
    {
        //return WebUtility.UrlEncode(str);
        return Uri.EscapeDataString(str);
    }

    /// <summary>
    /// Encodes a Uniform Resource Locator (URL) by replacing each instance of certain characters by one, two, three, or four escape sequences representing the UTF-8 encoding of the character.
    /// </summary>
    /// <param name="str">The URL to encode</param>
    /// <returns>URL encoded string</returns>
    [FunctionName("encodeUrl")]
    public static string EncodeUrl([AllowContextAsValue][PropagateUndefined] string str)
    {
        //see https://stackoverflow.com/a/34189188/376066 and 
        //    https://stackoverflow.com/questions/4396598/whats-the-difference-between-escapeuristring-and-escapedatastring/34189188#comment81544744_34189188
#pragma warning disable SYSLIB0013
        return Uri.EscapeUriString(str);
#pragma warning restore
    }

    /// <summary>
    /// Decodes a Uniform Resource Locator (URL) component previously created by encodeUrlComponent.
    /// </summary>
    /// <param name="str">The URL component to decode</param>
    /// <returns>Decoded string</returns>
    [FunctionName("decodeUrlComponent")]
    public static string DecodeUrlComponent([AllowContextAsValue][PropagateUndefined] string str)
    {
        //return WebUtility.UrlEncode(str);
        return Uri.UnescapeDataString(str);
    }

    /// <summary>
    /// Decodes a Uniform Resource Locator (URL) previously created by encodeUrl.
    /// </summary>
    /// <param name="str">The URL to decode</param>
    /// <returns>Decoded string</returns>
    [FunctionName("decodeUrl")]
    public static string DecodeUrl([AllowContextAsValue][PropagateUndefined] string str)
    {
        return Uri.UnescapeDataString(str); //there's no Uri.UnescapeUriString, but actually - what's the difference? 
        // see https://stackoverflow.com/questions/747641/what-is-the-difference-between-decodeuricomponent-and-decodeuri#comment116291569_747700
    }
}