using System.Text;

namespace Jsonata.Net.Native.Extensions;

/// <summary>
/// Extension methods for StringBuilder to support JSON formatting.
/// </summary>
internal static class StringBuilderExtensions
{
    /// <summary>
    /// Adds indentation spaces to the StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to append to</param>
    /// <param name="indent">The indentation level (each level adds 2 spaces)</param>
    internal static void Indent(this StringBuilder builder, int indent)
    {
        for (int i = 0; i < indent * 2; ++i)
        {
            builder.Append(' ');
        }
    }

    /// <summary>
    /// Appends a JSON line break (newline character) to the StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to append to</param>
    internal static void AppendJsonLine(this StringBuilder builder)
    {
        builder.Append('\n');
    }
}