using System;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Utility methods for string processing in the parsing context.
/// </summary>
internal static class StringUtils
{
    /// <summary>
    /// Replaces JSON escape sequences in a string with their unescaped equivalents.
    /// Valid escape sequences are:
    /// \X, where X is a character from jsonEscapes
    /// \uXXXX, where XXXX is a 4-digit hexadecimal Unicode code point.
    /// </summary>
    /// <param name="src">The string to unescape</param>
    /// <returns>The unescaped string</returns>
    internal static string Unescape(string src)
    {
        //https://stackoverflow.com/a/54355440/376066
        return System.Text.RegularExpressions.Regex.Unescape(src);
    }
}